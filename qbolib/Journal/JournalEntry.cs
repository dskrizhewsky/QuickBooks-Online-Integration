#region Copyright Notice
/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2023 Dmytro Skryzhevskyi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Automate.ExternalAccountingTools.Core;
using Automate.ExternalAccountingTools.Journal.BatchResults;
using Automate.ExternalAccountingTools.Journal.Entity;
using Intuit.Ipp.Core;
using Intuit.Ipp.Exception;
using Account = Automate.ExternalAccountingTools.Journal.Entity.Account;
using Customer = Automate.ExternalAccountingTools.Journal.Entity.Customer;
using QBOData = Intuit.Ipp.Data;
using Vendor = Automate.ExternalAccountingTools.Journal.Entity.Vendor;


namespace Automate.ExternalAccountingTools.Journal
{
    public class JournalEntry
    {
        private readonly string _batchId;
        private readonly decimal exchangeRateNotSpecifiedValue = decimal.MinValue;

        public JournalEntry()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lines"></param>
        /// <param name="currencyCode">In  ISO 4217 format</param>
        /// <param name="internalId"></param>
        /// <param name="description"></param>
        /// <param name="exchangeRate"></param>
        public JournalEntry(DateTime date, BalancedLines lines, Guid internalId, string description = "", string currencyCode = "CAD", decimal exchangeRate = decimal.MinValue) : this(date, lines, internalId)
        {
            if (date == null)
            {
                throw new ArgumentNullException("date argument should not be null");
            }

            if (lines == null)
            {
                throw new ArgumentNullException("lines argument should not be null");
            }

            if (internalId == Guid.Empty)
            {
                throw new Exception("internalId argument should have positive value.");
            }

            AssignOptionalData(currencyCode, description);
            ExchangeRate = exchangeRate;
        }

        public JournalEntry(string batchId, string id)
        {
            Id = id;
            InternalId = Guid.Parse(batchId.Split('_')[1]);
        }

        protected JournalEntry(DateTime date, BalancedLines lines, Guid internalId) : this()
        {
            AssignRequiredData(date, lines, internalId);
            _batchId = new BatchIdBuilder("je").GetBatchGuid(InternalId);
        }

        public string Id { get; private set; }
        public Guid InternalId { get; private set; }
        public DateTime Date { get; private set; }
        public BalancedLines Lines { get; private set; }
        public string CurrencyCode { get; private set; }
        public string Description { get; private set; }
        public decimal ExchangeRate { get; private set; }

        public string GetBatchId()
        {
            return _batchId;
        }

        private void AssignOptionalData(string currencyCode, string description)
        {
            CurrencyCode = currencyCode;
            Description = description;
           
        }

        public JournalEntry Add(DateTime date, BalancedLines lines,
            string memo, string description, Guid internalId)
        {
            if (lines == null) throw new ArgumentNullException("lines argument should not be null.");


            if (memo == null || description == null)
                throw new ArgumentException("Incorrect memo or description arguments.");
            AssignRequiredData(date, lines, internalId);
            AssignOptionalData(memo, description);
            return this;
        }

        public JournalEntry Add(DateTime date, BalancedLines lines, Guid internalId)
        {
            if (lines == null) throw new ArgumentNullException("lines argument should not be null.");


            AssignRequiredData(date, lines, internalId);
            return this;
        }

        private void VerifyArguments(Location location, BalancedLines lines)
        {
            if (location == null) throw new ArgumentNullException("location argument should not be null.");

            if (lines == null) throw new ArgumentNullException("lines argument should not be null.");
        }

        private void AssignRequiredData(DateTime date, BalancedLines lines, Guid internalId)
        {
            Date = date;
            Lines = lines;
            InternalId = internalId;
        }

        public string Execute(ServiceContext qboContext)
        {
            if (qboContext == null) throw new ArgumentNullException("ServiceContext should initialized in constructor");

            if (!Lines.AreBalanced()) throw new LinesNotBalancedException("JournalEntry lines are not balanced.");

            QBOData.JournalEntry entry = GetQboJournalEntry();
            QBOData.JournalEntry qboEntry = null;
            try
            {
                qboEntry = Helper.Add(qboContext, entry);
            }
            catch (ValidationException ex)
            {
            }
            catch (ServiceException ex)
            {
            }
            catch (SecurityException ex)
            {
            }
            catch (IdsException ex)
            {
            }

            return qboEntry?.Id;
        }

        private string GetCurrencyCode()
        {
            return string.IsNullOrEmpty(CurrencyCode) ? "CAD" : CurrencyCode;
        }

        public QBOData.JournalEntry GetQboJournalEntry()
        {
            QBOData.JournalEntry entry = new QBOData.JournalEntry
            {
                TxnDate = Date,
                TxnDateSpecified = true,
                CurrencyRef = new QBOData.ReferenceType { Value = GetCurrencyCode() },
             
            };

            if (ExchangeRate != exchangeRateNotSpecifiedValue)
            {
                entry.ExchangeRate = ExchangeRate;
                entry.ExchangeRateSpecified = true;
            }

            List<QBOData.Line> lines = new List<QBOData.Line>();
            lines.AddRange(Lines.CreditLines.Select(CreateLine));
            lines.AddRange(Lines.DebitLines.Select(CreateLine));

            entry.Line = lines.ToArray();
            return entry;
        }

        private QBOData.PostingTypeEnum JournalLineToPostingType(JournalLine journalLine)
        {
            return journalLine is CreditLine ? QBOData.PostingTypeEnum.Credit : QBOData.PostingTypeEnum.Debit;
        }

        private QBOData.Line CreateLine(JournalLine journalLine)
        {
            QBOData.Line line = new QBOData.Line();
            QBOData.JournalEntryLineDetail detail = new QBOData.JournalEntryLineDetail();
            detail.PostingType = JournalLineToPostingType(journalLine);
            detail.PostingTypeSpecified = true;
            detail.AccountRef = new QBOData.ReferenceType
            { name = journalLine.Account.Name, Value = journalLine.Account.Id };

            detail.DepartmentRef = new QBOData.ReferenceType
            { name = journalLine.Location.Name, Value = journalLine.Location.Id };

            if (!(journalLine.Name is EmptyReferenceName))
            {
                detail.Entity = new QBOData.EntityTypeRef();
                detail.Entity.EntityRef = new QBOData.ReferenceType
                    {Value = journalLine.Name.Id};

                detail.Entity.Type = journalLine.Name is Customer
                    ? QBOData.EntityTypeEnum.Customer
                    : QBOData.EntityTypeEnum.Vendor;

                detail.Entity.TypeSpecified = true;
            }

            line.AnyIntuitObject = detail;
            line.DetailType = QBOData.LineDetailTypeEnum.JournalEntryLineDetail;
            line.DetailTypeSpecified = true;
            line.Description = Description;
            line.Amount = journalLine.Amount;
            line.AmountSpecified = true;
            return line;
        }

        public bool HasConsistentQboObjects(VerificationResults verifiedObjects)
        {

            if (Lines.GetDistinctLocations().Any(location => verifiedObjects.Find(location) == null)) return false;

            if (Lines.GetDistinctVendors().Any(vendor => verifiedObjects.Find(vendor) == null)) return false;

            if (Lines.GetDistinctCustomers().Any(customer => verifiedObjects.Find(customer) == null)) return false;

            if (Lines.GetDistinctAccounts().Any(account => verifiedObjects.Find(account) == null)) return false;

            return true;
        }
    }

    public static class JournalEntryInconsistencyReport
    {
        public static ImmutableList<Location> GetInconsistentLocations(this JournalEntry entry, JournalEntriesBatchResults batchResults)
        {
                List<Location> result = new List<Location>(){};
                result.AddRange(from line in entry.Lines.CreditLines where batchResults.PreVerification.GetFailedLocations().Contains(line.Location) select line.Location);
                result.AddRange(from line in entry.Lines.DebitLines where batchResults.PreVerification.GetFailedLocations().Contains(line.Location) select line.Location);

                return result.ToImmutableList();

        }

        public static ImmutableList<Entity.Account> GetInconsistentAccounts(this JournalEntry entry, JournalEntriesBatchResults batchResults)
        {
            List<Entity.Account> result = new List<Entity.Account>() { };
            result.AddRange(from line in entry.Lines.CreditLines where batchResults.PreVerification.GetFailedAccounts().Contains(line.Account) select line.Account);
            result.AddRange(from line in entry.Lines.DebitLines where batchResults.PreVerification.GetFailedAccounts().Contains(line.Account) select line.Account);

            return result.ToImmutableList();

        }

        public static ImmutableList<Entity.Vendor> GetInconsistentVendors(this JournalEntry entry, JournalEntriesBatchResults batchResults)
        {
            List<Entity.Vendor> result = new List<Entity.Vendor>() { };
            IEnumerable<Entity.Vendor> range = from line in entry.Lines.CreditLines
                where batchResults.PreVerification.GetFailedVendors().Contains(line.Name)
                select line.Name as Entity.Vendor;
            result.AddRange(range);
         
            range = from line in entry.Lines.DebitLines
                where batchResults.PreVerification.GetFailedVendors().Contains(line.Name)
                select line.Name as Entity.Vendor;
            result.AddRange(range);
          

            return result.ToImmutableList();
        }
        public static ImmutableList<Entity.Customer> GetInconsistentCustomers(this JournalEntry entry, JournalEntriesBatchResults batchResults)
        {
            List<Entity.Customer> result = new List<Entity.Customer>() { };
          
            IEnumerable<Entity.Customer> range = from line in entry.Lines.CreditLines
                where batchResults.PreVerification.GetFailedCustomers().Contains(line.Name)
                select line.Name as Entity.Customer;
            result.AddRange(range);
           
            range = from line in entry.Lines.DebitLines
                where batchResults.PreVerification.GetFailedCustomers().Contains(line.Name)
                select line.Name as Entity.Customer;
            result.AddRange(range);

            return result.ToImmutableList();
        }
    }
}