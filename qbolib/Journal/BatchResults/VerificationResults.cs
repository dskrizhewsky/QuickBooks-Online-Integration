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
using Automate.ExternalAccountingTools.Journal.Entity;

namespace Automate.ExternalAccountingTools.Journal.BatchResults
{
    public class VerificationResults
    {
        private readonly List<Intuit.Ipp.Data.Customer> _customers = new List<Intuit.Ipp.Data.Customer>();
        private readonly List<Intuit.Ipp.Data.Vendor> _vendors = new List<Intuit.Ipp.Data.Vendor>();
        private readonly List<Intuit.Ipp.Data.Department> _departments = new List<Intuit.Ipp.Data.Department>();
        private readonly List<Intuit.Ipp.Data.Account> _accounts = new List<Intuit.Ipp.Data.Account>();
        
        private readonly List<Customer> _failedCustomers = new List<Customer>();
        private readonly List<Vendor> _failedVendors = new List<Vendor>();
        private readonly List<Location> _failedLocations = new List<Location>();
        private readonly List<Account> _failedAccounts = new List<Account>();

        public ImmutableList<Customer> GetFailedCustomers()
        {
            return _failedCustomers.ToImmutableList();
        }

        public ImmutableList<Vendor> GetFailedVendors()
        {
            return _failedVendors.ToImmutableList();
        }

        public ImmutableList<Location> GetFailedLocations()
        {
            return _failedLocations.ToImmutableList();
        }

        public ImmutableList<Account> GetFailedAccounts()
        {
            return _failedAccounts.ToImmutableList();
        }

        public void Add(Intuit.Ipp.Data.Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer argument should not be null");
            }
            _customers.Add(customer);
        }

        public void Add(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer argument should not be null");
            }
            _failedCustomers.Add(customer);
        }



        public Intuit.Ipp.Data.Customer Find(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer argument should not be null");
            }

            return _customers.Find(a => a.Id == customer.Id || a.DisplayName == customer.DisplayName);
        }

        private void VerifyVendor(Intuit.Ipp.Data.Customer vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException("vendor argument should not be null");
            }
        }

        public void Add(Intuit.Ipp.Data.Vendor vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException("vendor argument should not be null");
            }

            _vendors.Add(vendor);
        }

        public void Add(Vendor vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException("vendor argument should not be null");
            }

            _failedVendors.Add(vendor);
        }

        public Intuit.Ipp.Data.Vendor Find(Vendor vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException("vendor argument should not be null");
            }
            return _vendors.Find(a => a.Id == vendor.Id || a.DisplayName == vendor.DisplayName);
        }

        public void Add(Intuit.Ipp.Data.Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException("department argument should not be null");
            }
            _departments.Add(department);
        }

        public void Add(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("department argument should not be null");
            }
            _failedLocations.Add(location);
        }

        public Intuit.Ipp.Data.Department Find(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location argument should not be null");
            }
            return _departments.Find(a => a.Id == location.Id  || a.Name == location.Name);
        }

        public void Add(Intuit.Ipp.Data.Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account argument should not be null");
            }
            _accounts.Add(account);
        }

        public void Add(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account argument should not be null");
            }
            _failedAccounts.Add(account);
        }

        public Intuit.Ipp.Data.Account Find(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account argument should not be null");
            }

            return _accounts.Find(a => a.Id == account.Id || a.Name == account.Name);
        }
    }
}