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
using Automate.ExternalAccountingTools.Core;
using QBOData = Intuit.Ipp.Data;

namespace Automate.ExternalAccountingTools.Journal.Entity
{
    public class Customer : ReferenceName, IEquatable<Customer>, IEqualityComparer<Customer>
    {
        public Customer(string id, string displayName)
        {
           
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect id name argument.");
            }
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentException($"{displayName ?? "NULL"}: Incorrect displayName name argument.");
            }

            Id = id;
            DisplayName = displayName;

        }

        public Customer(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Incorrect customer ID argument.");
            }

            Id = id.ToString();

        }

        public Customer(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{name ?? "NULL"}: Incorrect customer name argument.");
            }

            DisplayName = name;
            Id = ObjectCache.GetInstance().GetCustomerId(name);

        }

        public QBOData.Customer GetQboCustomer()
        {
            return new QBOData.Customer() { Id = Id };
        }

        public string GetQuery()
        {
            QboQueryBuilder builder = new QboQueryBuilder("Customer");
            return string.IsNullOrEmpty(Id) ? builder.GetSelectByDisplayName(DisplayName) : builder.GetSelectById(Id);
        }

        public string GetBatchId()
        {
            BatchIdBuilder builder = new BatchIdBuilder("c");
            return string.IsNullOrEmpty(Id) ? builder.GetBatchId(DisplayName) : builder.GetBatchId(Id);
        }


        public bool Equals(Customer other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (this.Id == other.Id && this.DisplayName == other.DisplayName)
            {
                return true;
            }

            return false;
        }

        public bool Equals(Customer x, Customer y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x.Id == y.Id && x.DisplayName ==y.DisplayName)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(Customer obj)
        {
            return new Hash().Calculate(obj.Id, obj.DisplayName);
        }

        public override int GetHashCode()
        {
            return new Hash().Calculate(Id, DisplayName);
        }
    }
}
