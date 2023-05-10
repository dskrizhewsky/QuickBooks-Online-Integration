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
using Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity;

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal
{
    public class BalancedLines
    {
        private BalancedLines(){}
        public BalancedLines(IList<DebitLine> debitLines, IList<CreditLine> creditLines)
        {
            if (debitLines == null || creditLines == null)
            {
                throw new ArgumentNullException("Lines arguments should not be null");
            }

            if (!debitLines.Any() || !creditLines.Any())
            {
                throw new ArgumentException("Each lines arguments should contain at least 1 item ");
            }

            DebitLines = debitLines.ToImmutableList();
            CreditLines = creditLines.ToImmutableList();
        }

        public bool AreBalanced()
        {
            if (!DebitLines.Any() || !CreditLines.Any())
            {
                return false;
            }

            decimal debit = DebitLines.Sum(a => a.Amount);
            decimal credit = CreditLines.Sum(a => a.Amount);
            return debit == credit;
        }

        public ImmutableList<Account> GetDistinctAccounts()
        {
            List<Account> lines = new List<Account>();
            lines.AddRange(DebitLines.Select(line => line.Account));
            lines.AddRange(CreditLines.Select(line => line.Account));
            return lines.Distinct().ToImmutableList();
        }

        public ImmutableList<Customer> GetDistinctCustomers()
        {
            List<Customer> lines = new List<Customer>();
            lines.AddRange(DebitLines.Select(line => line.Name).Where(line => line is Customer).Cast<Customer>());
            lines.AddRange(CreditLines.Select(line => line.Name).Where(line => line is Customer).Cast<Customer>());
            return lines.Distinct().ToImmutableList();
        }

        public ImmutableList<Vendor> GetDistinctVendors()
        {
            List<Vendor> lines = new List<Vendor>();
            lines.AddRange(DebitLines.Select(line => line.Name).Where(line => line is Vendor).Cast<Vendor>());
            lines.AddRange(CreditLines.Select(line => line.Name).Where(line => line is Vendor).Cast<Vendor>());
            return lines.Distinct().ToImmutableList();
        }

        public ImmutableList<Location> GetDistinctLocations()
        {
            List<Location> lines = new List<Location>();
            lines.AddRange(DebitLines.Select(line => line.Location));
            lines.AddRange(CreditLines.Select(line => line.Location));
            return lines.Distinct().ToImmutableList();
        }

        public ImmutableList<DebitLine> DebitLines { get; }
        public ImmutableList<CreditLine> CreditLines { get; }
    }
}
