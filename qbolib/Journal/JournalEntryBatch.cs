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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.BatchResults;
using Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;
using Account = Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity.Account;
using Customer = Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity.Customer;
using QBOData = Intuit.Ipp.Data;
using Vendor = Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity.Vendor;

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal
{
    internal class JournalEntryBatch
    {
        private readonly List<JournalEntry> _journalEntryCollection;

        public JournalEntryBatch()
        {
            _journalEntryCollection = new List<JournalEntry>();
        }

        public JournalEntriesBatchResults JournalEntriesBatchResults
        {
            get => default;
            set
            {
            }
        }

        public void Clear()
        {
            _journalEntryCollection.Clear();
        }

        public void Add(JournalEntry journalEntry)
        {
            if (journalEntry == null)
            {
                throw new ArgumentNullException("journalEntry argument should not be null");
            }

            if (!journalEntry.Lines.AreBalanced())
            {
                throw new LinesNotBalancedException("JournalEntry lines are not balanced.");
            }

            if (_journalEntryCollection.Count >= GetMaxRequestPayloads() || GetDependentEntityCount() >= GetMaxRequestPayloads())
            {
                Debug.WriteLine($"Entries: {_journalEntryCollection.Count} Dependent: {GetDependentEntityCount()}");
                throw new RequestPayloadMaximumException($"The maximum number of payloads in a single batch request is {GetMaxRequestPayloads()}");
            }



            _journalEntryCollection.Add(journalEntry);
        }

        private int GetMaxRequestPayloads()
        {
            return 25;
        }


        private int GetDependentEntityCount()
        {
            return GetDistinctAccounts().Count() + GetDistinctCustomers().Count() + GetDistinctLocations().Count() +
                   GetDistinctVendors().Count();
        }

        public ImmutableList<Entity.Vendor> GetDistinctVendors()
        {
            List<Entity.Vendor> allVendors = new List<Entity.Vendor>();
            foreach (ImmutableList<Entity.Vendor> vendors in _journalEntryCollection.Select(entry => entry.Lines.GetDistinctVendors()))
            {
                allVendors.AddRange(vendors);
            }

            return allVendors.Distinct().ToImmutableList();
        }

        public ImmutableList<Location> GetDistinctLocations()
        {

            List<Location> allLocations = new List<Location>();
            foreach (ImmutableList<Location> location in _journalEntryCollection.Select(entry => entry.Lines.GetDistinctLocations()))
            {
                allLocations.AddRange(location);
            }

            return allLocations.Distinct().ToImmutableList();
        }

        public ImmutableList<Entity.Customer> GetDistinctCustomers()
        {
            List<Entity.Customer> allCustomers = new List<Entity.Customer>();
            foreach (ImmutableList<Entity.Customer> customers in _journalEntryCollection.Select(entry => entry.Lines.GetDistinctCustomers()))
            {
                allCustomers.AddRange(customers);
            }

            return allCustomers.Distinct().ToImmutableList();
        }

        public ImmutableList<Entity.Account> GetDistinctAccounts()
        {
            List<Entity.Account> allAccounts = new List<Entity.Account>();
            foreach (ImmutableList<Entity.Account> accounts in _journalEntryCollection.Select(entry => entry.Lines.GetDistinctAccounts()))
            {
                allAccounts.AddRange(accounts);
            }

            return allAccounts.Distinct().ToImmutableList();
        }



        public JournalEntriesBatchResults BatchVerifyAndCreate(ServiceContext qboContext)
        {
            if (qboContext == null)
            {
                throw new ArgumentNullException("ServiceContext argument should not be null.");
            }

            VerificationResults verifiedObject = BatchVerify(qboContext);
            JournalEntriesBatchResults result = new JournalEntriesBatchResults(verifiedObject);
            List<JournalEntry> consistentJournalEntries = new List<JournalEntry>();

            DataService service = new DataService(qboContext);
            Batch batch = service.CreateNewBatch();
            foreach (JournalEntry journalEntry in _journalEntryCollection)
            {
                
                if (journalEntry.HasConsistentQboObjects(verifiedObject))
                {
                    consistentJournalEntries.Add(journalEntry);
                    batch.Add(journalEntry.GetQboJournalEntry(), journalEntry.GetBatchId(), QBOData.OperationEnum.create);
                }
                else
                {
                    result.AddInconsistent(journalEntry);
                }

            }

            if (consistentJournalEntries.Any())
            {
                ThrottleExecute(batch);
            }

            foreach (JournalEntry journalEntry in consistentJournalEntries)
            {
                IntuitBatchResponse response = batch[journalEntry.GetBatchId()];
                if (response.ResponseType == ResponseType.Entity)
                {
                    QBOData.JournalEntry qboJournalEntry = response.Entity as QBOData.JournalEntry;
                    result.AddAdded(new JournalEntry(journalEntry.GetBatchId(), qboJournalEntry?.Id));
                }
                else
                {
                    string errorDescription = string.Empty;
                    if (response.ResponseType == ResponseType.Exception)
                    {
                        errorDescription = GetFormattedException(response);
                    }

                    result.AddFailed(journalEntry, errorDescription);
                }
            }

            return result;
        }

        private string GetFormattedException(IntuitBatchResponse response)
        {
            string errorDescription = $"Code: {response.Exception.ErrorCode} Message: {response.Exception.Message}";
            return errorDescription;
        }

        private VerificationResults BatchVerify(ServiceContext qboContext)
        {
            DataService service = new DataService(qboContext);
            Batch batch = service.CreateNewBatch();
            LoadBatchRequests(batch);
            ThrottleExecute(batch);
            VerificationResults verifiedCollection = ProcessBatchResponse(batch);
            return verifiedCollection;
        }

        private int MaxBatchRequestsPerMinute()
        {
            //https://developer.intuit.com/app/developer/qbo/docs/develop/rest-api-features#limits-and-throttles
            //Throttled at 40 requests per minute, per realm ID.
            return 30; //Just in case the docs are outdated
        }

        private void ThrottleExecute(Batch batch)
        {
            Stopwatch watch = Stopwatch.StartNew();
            batch.Execute();
            watch.Stop();
            double passed = watch.Elapsed.TotalMilliseconds;
            int recommended = 1000 * 60 / MaxBatchRequestsPerMinute();
            if (passed < recommended)
            {
                int delay = (int) (recommended - passed);
                Thread.Sleep(delay);
            }

        }


        private VerificationResults ProcessBatchResponse(Batch batch)
        {
            VerificationResults versificationResults = new VerificationResults();
            foreach (Entity.Customer customer in GetDistinctCustomers())
            {
                IntuitBatchResponse response = batch[customer.GetBatchId()];
                if (!response.Entities.Any() || response.ResponseType == ResponseType.Exception)
                {
                    versificationResults.Add(customer);
                }
                else if (response.ResponseType == ResponseType.Query)
                {
                    QBOData.Customer qboCustomer = response.Entities.FirstOrDefault() as QBOData.Customer;
                    versificationResults.Add(qboCustomer);
                }
            }

            foreach (Entity.Vendor vendor in GetDistinctVendors())
            {
                IntuitBatchResponse response = batch[vendor.GetBatchId()];
                if (!response.Entities.Any() || response.ResponseType == ResponseType.Exception)
                {
                    versificationResults.Add(vendor);
                }
                else if (response.ResponseType == ResponseType.Query)
                {
                    QBOData.Vendor qboVendor = response.Entities.FirstOrDefault() as QBOData.Vendor;
                    versificationResults.Add(qboVendor);
                }
            }

            foreach (Location location in GetDistinctLocations())
            {
                IntuitBatchResponse response = batch[location.GetBatchId()];
                if (!response.Entities.Any() || response.ResponseType == ResponseType.Exception)
                {
                    versificationResults.Add(location);
                }
                else if (response.ResponseType == ResponseType.Query)
                {
                    QBOData.Department qboLocation = response.Entities.FirstOrDefault() as QBOData.Department;
                    versificationResults.Add(qboLocation);
                }
            }

            foreach (Entity.Account account in GetDistinctAccounts())
            {
                IntuitBatchResponse response = batch[account.GetBatchId()];
                if (!response.Entities.Any() || response.ResponseType == ResponseType.Exception)
                {
                    versificationResults.Add(account);
                }
                else if (response.ResponseType == ResponseType.Query)
                {
                    QBOData.Account qboAccount = response.Entities.FirstOrDefault() as QBOData.Account;
                    versificationResults.Add(qboAccount);
                }
            }

            return versificationResults;

        }

        private void LoadBatchRequests(Batch batch)
        {
            foreach (Entity.Customer customer in GetDistinctCustomers())
            {

                batch.Add(customer.GetQuery(), customer.GetBatchId());
            }

            foreach (Entity.Vendor vendor in GetDistinctVendors())
            {
                batch.Add(vendor.GetQuery(), vendor.GetBatchId());
            }

            foreach (Location location in GetDistinctLocations())
            {
                batch.Add(location.GetQuery(), location.GetBatchId());
            }

            foreach (Entity.Account account in GetDistinctAccounts())
            {
                batch.Add(account.GetQuery(), account.GetBatchId());
            }
        }
    }
}
