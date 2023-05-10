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
using Dmytro.Skryzhevskyi.ExternalAccountingTools.Core;
using QBOData = Intuit.Ipp.Data;

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity
{
    public class Account : IEquatable<Account>, IEqualityComparer<Account>
    {
        private Account()
        {
            IsPayable = false;
            IsReceivable = false;
        }


        public Account(string id, string name, QBOData.AccountTypeEnum accountType, bool accountPayable, bool accountReceivable) : base()
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect account id");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{name ?? "NULL"}: Incorrect account name");
            }

            Id = id;
            Name = name;
            IsPayable = accountPayable;
            IsReceivable = accountReceivable;
        }
        public Account(Guid id) : base()
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Incorrect account ID");
            }

            Id = id.ToString();

        }
        public Account(int id) : base()
        {
            const int minAccountId = 0;
            if (id <= minAccountId)
            {
                throw new ArgumentOutOfRangeException("Account number should be positive integer number");
            }
            var account = ObjectCache.GetInstance().GetAccountById(id);
            Id = account.Id;
            Name = account.Name;
            IsReceivable = account.IsReceivable;
            IsPayable = account.IsPayable;
        }
        public Account(string name) : base()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{name ?? "NULL"}: Incorrect account name");
            }

            Name = name;
            var account = ObjectCache.GetInstance().GetAccount(name);
            Id = account.Id;
            IsReceivable = account.IsReceivable;
            IsPayable = account.IsPayable;

        }
        public string Id { get; private set; }
        public string Name { get; private set; }


        public QBOData.Account GetQboAccount()
        {
            return new QBOData.Account() { Id = Id };
        }

        public string GetQuery()
        {
            QboQueryBuilder builder = new QboQueryBuilder("Account");
            return string.IsNullOrEmpty(Id) ? builder.GetSelectByName(Name) : builder.GetSelectById(Id);
        }

        public string GetBatchId()
        {
            BatchIdBuilder builder = new BatchIdBuilder("a");
            return string.IsNullOrEmpty(Id) ? builder.GetBatchId(Name) : builder.GetBatchId(Id);
        }


        public override int GetHashCode()
        {
            return new Hash().Calculate(Id, Name);
        }

        public bool IsPayable { get; private set; }
        public bool IsReceivable { get; private set; }

        public bool Equals(Account other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (Id == other.Id && Name == other.Name)
            {
                return true;
            }

            return false;
        }

        public bool Equals(Account x, Account y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x.Id == y.Id && x.Name == y.Name)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(Account obj)
        {
            return new Hash().Calculate(obj.Id, obj.Name);
        }
    }
}
