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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automate.ExternalAccountingTools.Core;
using Automate.ExternalAccountingTools.Journal;
using Automate.ExternalAccountingTools.Journal.BatchResults;
using Automate.ExternalAccountingTools.Journal.Entity;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.QueryFilter;
using QBOData = Intuit.Ipp.Data;

namespace Automate.ExternalAccountingTools
{
    public interface IExternalRepository<T>
    {
        Task<IReadOnlyCollection<T>> GetAllAsync();
        Task<T> FindByNameAsync(string name);
    }

    public class LocationRepository : IExternalRepository<Location>
    {
        private readonly ServiceContext _ctx;
        public LocationRepository(ServiceContext ctx)
        {
            _ctx = ctx;
        }

        public Task<IReadOnlyCollection<Location>> GetAllAsync()
        {
            VerifyServiceContext();
            var qboDepartmentCollection = Helper.LoadFullCollection(_ctx, new QBOData.Department());

            var result = qboDepartmentCollection.Select(qboDepartment =>
                new Location(_ctx, qboDepartment.Id, qboDepartment.Name)).OrderBy(a => a.Name).ToList();
            IReadOnlyCollection<Location> locations = new ReadOnlyCollection<Location>(result.AsReadOnly());

            return Task.FromResult(locations);
        }

        public Task<Location> FindByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Incorrect name argument");
            }

            VerifyServiceContext();
            var querySvc = new QueryService<QBOData.Department>(_ctx);
            var qboDepartment = querySvc.ExecuteIdsQuery(new QboQueryBuilder("Department").GetSelectByName(name)).FirstOrDefault();
            return Task.FromResult(Create(qboDepartment));
        }

        public Task<Location> FindByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect id argument");
            }

            VerifyServiceContext();
            var qboDepartment = Helper.FindById(_ctx, new QBOData.Department() { Id = id });
            return Task.FromResult(Create(qboDepartment));
        }

        private void VerifyServiceContext()
        {
            if (_ctx == null)
            {
                throw new ArgumentNullException("ServiceContext should initialized in constructor");
            }
        }

        private Location Create(QBOData.Department qboDepartment)
        {
            VerifyServiceContext();
            if (qboDepartment == null) return null;
            return new Location(_ctx, qboDepartment.Id, qboDepartment.Name);
        }
    }

    public class AccountsRepository : IExternalRepository<Account>
    {
        private readonly ServiceContext _ctx;
        public AccountsRepository(ServiceContext ctx)
        {
            _ctx = ctx;
        }
        public Task<IReadOnlyCollection<Account>> GetAllAsync()
        {
            VerifyServiceContext();
            var qboAccountCollection = Helper.LoadFullCollection(_ctx, new QBOData.Account());
            var result = qboAccountCollection.Select(qboAccount =>
                new Account(qboAccount.Id, qboAccount.Name, qboAccount.AccountType,
                    qboAccount.AccountType == QBOData.AccountTypeEnum.AccountsPayable, qboAccount.AccountType == QBOData.AccountTypeEnum.AccountsReceivable)).
                OrderBy(a => a.Name).ToList();
            IReadOnlyCollection<Account> accounts = new ReadOnlyCollection<Account>(result.AsReadOnly());
            return Task.FromResult(accounts);
        }

        public Task<Account> FindByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Incorrect account name");
            }

            VerifyServiceContext();
            var querySvc = new QueryService<QBOData.Account>(_ctx);
            var qboAccount = querySvc.ExecuteIdsQuery(new QboQueryBuilder("Account").GetSelectByName(name)).FirstOrDefault();
            return Task.FromResult(Create(qboAccount));
        }

        public Task<Account> FindByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect ID argument.");
            }
            VerifyServiceContext();
            var qboAccount = Helper.FindById(_ctx, new QBOData.Account() { Id = id });
            return Task.FromResult(Create(qboAccount));
        }

        private void VerifyServiceContext()
        {
            if (_ctx == null)
            {
                throw new ArgumentNullException("ServiceContext should initialized in constructor");
            }
        }

        private Account Create(QBOData.Account qboAccount)
        {
            if (qboAccount == null) return null;

            VerifyServiceContext();
            return new Account(qboAccount.Id, qboAccount.Name, qboAccount.AccountType,
                qboAccount.AccountType == QBOData.AccountTypeEnum.AccountsPayable, qboAccount.AccountType == QBOData.AccountTypeEnum.AccountsReceivable);
        }
    }

    public class CustomersRepository : IExternalRepository<Customer>
    {
        private readonly ServiceContext _ctx;
        public CustomersRepository(ServiceContext ctx)
        {
            _ctx = ctx;
        }
        public Task<IReadOnlyCollection<Customer>> GetAllAsync()
        {
            VerifyServiceContext();
            var qboCustomerCollection = Helper.LoadFullCollection(_ctx, new QBOData.Customer());

            var result = qboCustomerCollection.Select(qboCustomer =>
                new Customer(qboCustomer.Id, qboCustomer.DisplayName)).OrderBy(a => a.DisplayName).ToList();
            IReadOnlyCollection<Customer> customers = new ReadOnlyCollection<Customer>(result.AsReadOnly());
            return Task.FromResult(customers);
        }

        public Task<Customer> FindByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Incorrect displayName argument.");
            }

            VerifyServiceContext();
            var querySvc = new QueryService<Intuit.Ipp.Data.Customer>(_ctx);
            var qboCustomer = querySvc.ExecuteIdsQuery(new QboQueryBuilder("Customer").GetSelectByDisplayName(name)).FirstOrDefault();
            return Task.FromResult(Create(qboCustomer));
        }

        private void VerifyServiceContext()
        {
            if (_ctx == null)
            {
                throw new ArgumentNullException("ServiceContext should initialized in constructor");
            }
        }

        private Customer Create(QBOData.Customer qboCustomer)
        {
            VerifyServiceContext();
            if (qboCustomer == null) return null;
            return new Customer(qboCustomer.Id, qboCustomer.DisplayName);

        }

        public Task<Customer> FindByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect ID argument.");
            }
            VerifyServiceContext();
            var qboCustomer = Helper.FindById(_ctx, new QBOData.Customer { Id = id });
            return Task.FromResult(Create(qboCustomer));
        }
    }

    public class VendorsRepository : IExternalRepository<Vendor>
    {
        private readonly ServiceContext _ctx;
        public VendorsRepository(ServiceContext ctx)
        {
            _ctx = ctx;
        }

        public Task<IReadOnlyCollection<Vendor>> GetAllAsync()
        {
            VerifyServiceContext();
            var qboVendorCollection = Helper.LoadFullCollection(_ctx, new QBOData.Vendor());

            var result = qboVendorCollection.Select(qboVendor =>
                new Vendor(qboVendor.Id, qboVendor.DisplayName)).OrderBy(a => a.DisplayName).ToList();
            IReadOnlyCollection<Vendor> vendors = new ReadOnlyCollection<Vendor>(result.AsReadOnly());

            return Task.FromResult(vendors);
        }

        public Task<Vendor> FindByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Incorrect displayName argument.");
            }

            VerifyServiceContext();
            var querySvc = new QueryService<QBOData.Vendor>(_ctx);
            var qboVendor = querySvc.ExecuteIdsQuery(new QboQueryBuilder("Vendor").GetSelectByDisplayName(name)).FirstOrDefault();
            return Task.FromResult(Create(qboVendor));

        }

        public Task<Vendor> FindByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect ID argument.");
            }

            VerifyServiceContext();
            var qboVendor = Helper.FindById(_ctx, new QBOData.Vendor() { Id = id });
            return Task.FromResult(Create(qboVendor));
        }

        private Vendor Create(QBOData.Vendor qboVendor)
        {
            VerifyServiceContext();
            if (qboVendor == null) return null;
            return new Vendor(qboVendor.Id, qboVendor.DisplayName);
        }

        private void VerifyServiceContext()
        {
            if (_ctx == null)
            {
                throw new ArgumentNullException("ServiceContext should initialized in constructor");
            }
        }
    }



    public interface IExternalAccountingContext
    {
        IExternalRepository<Account> AccountsRepository { get; }
        IExternalRepository<Customer> CustomersRepository { get; }
        IExternalRepository<Location> LocationsRepository { get; }
        IExternalRepository<Vendor> VendorsRepository { get; }

        void AddJournalEntry(JournalEntry journalEntry);
        Task<JournalEntriesBatchResults> UploadDataAsync();
    }

    public class QBOContext : IExternalAccountingContext
    {
        private readonly ServiceContext _ctx;
        private readonly JournalEntryBatch _batch = new JournalEntryBatch();
        public QBOContext(ServiceContext ctx)
        {
            _ctx = ctx;

            AccountsRepository = new AccountsRepository(_ctx);
            LocationsRepository = new LocationRepository(_ctx);
            CustomersRepository = new CustomersRepository(_ctx);
            VendorsRepository = new VendorsRepository(_ctx);
            InitCache();
        }
        public IExternalRepository<Account> AccountsRepository { get; }
        public IExternalRepository<Customer> CustomersRepository { get; }
        public IExternalRepository<Location> LocationsRepository { get; }
        public IExternalRepository<Vendor> VendorsRepository { get; }
        public void AddJournalEntry(JournalEntry journalEntry)
        {
            if (journalEntry == null)
            {
                throw new ArgumentNullException("journalEntry argument should not be null");
            }

            _batch.Add(journalEntry);
        }

        public Task<JournalEntriesBatchResults> UploadDataAsync()
        {
            var results = _batch.BatchVerifyAndCreate(_ctx);
            return Task.FromResult(results);
        }

        private void InitCache()
        {
            var id = _ctx.RealmId;
            ObjectCache.GetInstance().ClearCache();
            ObjectCache.GetInstance().Add(CustomersRepository.GetAllAsync().Result)
                .Add(VendorsRepository.GetAllAsync().Result)
                .Add(AccountsRepository.GetAllAsync().Result)
                .Add(LocationsRepository.GetAllAsync().Result);
        }
    }

    public static class ExternalUowFactory
    {
        public static IExternalAccountingContext CreateQBContext(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("accessToken argument should be defined.");
            }

            var auth = new Authorization(new AuthConfigSandbox());
            var ctx = auth.CreateContext(new Token(accessToken, 0, "", 0));
            ctx.IppConfiguration.RetryPolicy = new IntuitRetryPolicy(ctx, 5, new TimeSpan(0, 0, 0, 2), new TimeSpan(0, 0, 0, 2));
            InitAdvancedLogging(ctx);


            return new QBOContext(ctx);
        }

        private static void InitAdvancedLogging(ServiceContext ctx)
        {
            var advancedLogger = new AdvancedLogger
            {
                RequestAdvancedLog = new RequestAdvancedLog
                {
                    EnableSerilogRequestResponseLoggingForDebug = true,
                    EnableSerilogRequestResponseLoggingForConsole = true,
                    EnableSerilogRequestResponseLoggingForTrace = true
                }
            };

            ctx.IppConfiguration.AdvancedLogger = advancedLogger;
            ctx.IppConfiguration.Logger.RequestLog.EnableRequestResponseLogging = true;
        }

        private static void InitFileLogging(ServiceContext ctx, string fileName)
        {
            ctx.IppConfiguration.Logger.RequestLog.EnableRequestResponseLogging = true;
            ctx.IppConfiguration.Logger.RequestLog.ServiceRequestLoggingLocation = fileName;
        }
    }


}
