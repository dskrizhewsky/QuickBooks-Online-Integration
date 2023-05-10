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
using System.Linq;
using Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal;
using Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity;

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools
{
    internal class ObjectCache
    {
        private static ObjectCache _instance = new ObjectCache();
        private static readonly List<ObjectCache> _instances = new List<ObjectCache>();
        private readonly string _cashId;
        private readonly Dictionary<string, string> _customers = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _vendors = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _locations = new Dictionary<string, string>();
        private readonly Dictionary<string, Account> _accounts = new Dictionary<string, Account>();
        private readonly Dictionary<string, Account> _accountsById = new Dictionary<string, Account>();


        private ObjectCache()
        {
        }

        private ObjectCache(string cashId)
        {
            _cashId = cashId;
        }

        public void ClearCache()
        {
            _customers.Clear();
            _vendors.Clear();
            _locations.Clear();
            _accounts.Clear();
            _accountsById.Clear();
        }

        public ObjectCache Add(IEnumerable<Customer> customers)
        {
            if (customers == null)
            {
                throw new ArgumentNullException("customers argument should be defined.");
            }

            foreach (var customer in customers)
            {
                _customers.Add(customer.DisplayName.ToLowerInvariant(), customer.Id);
            }

            return _instance;
        }

        public ObjectCache Add(IEnumerable<Vendor> vendors)
        {
            if (vendors == null)
            {
                throw new ArgumentNullException("vendors argument should be defined.");
            }

            foreach (var vendor in vendors)
            {
                _vendors.Add(vendor.DisplayName.ToLowerInvariant(), vendor.Id);
            }

            return _instance;
        }

        public ObjectCache Add(IEnumerable<Account> accounts)
        {
            if (accounts == null)
            {
                throw new ArgumentNullException("accounts argument should be defined.");
            }

            foreach (var account in accounts)
            {
                _accounts.Add(account.Name.ToLowerInvariant(), account);
                _accountsById.Add(account.Id, account);
            }

            return _instance;
        }

        public ObjectCache Add(IEnumerable<Location> locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations argument should be defined.");
            }

            foreach (var location in locations)
            {
                _locations.Add(location.Name.ToLowerInvariant(), location.Id);
            }

            return _instance;
        }

        public static ObjectCache GetInstance(string id)
        {
            var foundInstances = _instances.Where(a => a._cashId == id).ToArray();
            if (!foundInstances.Any())
            {
                var instance = new ObjectCache(id);
                _instances.Add(instance);
                return instance;
            }

            return foundInstances.FirstOrDefault();
        }

        public static ObjectCache GetInstance()
        {
            return _instance ?? (_instance = new ObjectCache());
        }

        public string GetCustomerId(string name)
        {
            string customerName = name?.ToLowerInvariant();

            VerifyCache();
            VerifyArgument(customerName);
            var found = _customers.TryGetValue(customerName, out var id);
            if (!found)
            {
                throw new CachedObjectNotFoundException($"Customer {name} not found.");
            }

            return id;
        }

        public string GetVendorId(string name)
        {
            string vendorName = name?.ToLowerInvariant();

            VerifyCache();
            VerifyArgument(vendorName);
            var found = _vendors.TryGetValue(vendorName, out var id);
            if (!found)
            {
                throw new CachedObjectNotFoundException($"Vendor {name} not found.");
            }

            return id;
        }

        public string GetLocationId(string name)
        {
            string locationName = name?.ToLowerInvariant();

            VerifyCache();
            VerifyArgument(locationName);
            var found = _locations.TryGetValue(locationName, out var id);
            if (!found)
            {
                throw new CachedObjectNotFoundException($"Location {name} not found.");
            }

            return id;
        }

        public Account GetAccountById(int id)
        {
            VerifyAccountId(id);
            VerifyCache();
            var found = _accountsById.TryGetValue(id.ToString(), out var account);
            if (!found)
            {
                throw new CachedObjectNotFoundException($"Account {id} not found.");
            }

            return account;
        }

        private static void VerifyAccountId(int id)
        {
            const int minAccountId = 0;
            if (id <= minAccountId)
            {
                throw new ArgumentOutOfRangeException("Account number should be positive integer number");
            }
        }

        public Account GetAccount(string name)
        {
            string accountName = name?.ToLowerInvariant();

            VerifyCache();
            VerifyArgument(accountName);
            var found = _accounts.TryGetValue(accountName, out var account);
            if (!found)
            {
                throw new CachedObjectNotFoundException($"Account {name} not found.");
            }

            return account;
        }

        private void VerifyArgument(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name argument should be defined.");
            }
        }

        private bool IsCacheInitialized()
        {
            return _customers.Any() && _vendors.Any() && _accounts.Any() && _locations.Any();
        }

        private void VerifyCache()
        {
            if (!IsCacheInitialized())
            {
                throw new CachedObjectsNotLoadedException("Cache objects are not loaded.");
            }
        }
    }
}