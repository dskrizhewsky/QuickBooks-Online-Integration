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
using Intuit.Ipp.Core;
using QBOData = Intuit.Ipp.Data;

namespace Automate.ExternalAccountingTools.Journal.Entity
{


    public class Location : IEquatable<Location>, IEqualityComparer<Location>
    {
        public Location(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Incorrect location ID");
            }

            Id = id.ToString();
        }

        public Location(ServiceContext qboContext, string id, string name)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect account id");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{name ?? "NULL"}: Incorrect location name");
            }

            Id = id;
            Name = name;
        }

        public Location(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{name ?? "NULL"}: Incorrect location name");
            }

            Name = name;
            Id = ObjectCache.GetInstance().GetLocationId(name);
        }

        public string Id { get; private set; }
        public string Name { get; private set; }

        public QBOData.Department GetQboLocation()
        {
            return new QBOData.Department() { Id = Id };
        }

        public string GetQuery()
        {
            QboQueryBuilder builder = new QboQueryBuilder("Department");
            return string.IsNullOrEmpty(Id) ? builder.GetSelectByName(Name) : builder.GetSelectById(Id);
        }

        public string GetBatchId()
        {
            BatchIdBuilder builder = new BatchIdBuilder("l");
            return string.IsNullOrEmpty(Id) ? builder.GetBatchId(Name) : builder.GetBatchId(Id);
        }

        public bool Equals(Location other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (this.Id == other.Id && this.Name == other.Name)
            {
                return true;
            }

            return false;
        }

        public bool Equals(Location x, Location y)
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

        public int GetHashCode(Location obj)
        {
            return new Hash().Calculate(obj.Id, obj.Name);
        }

        public override int GetHashCode()
        {
            return new Hash().Calculate(Id, Name);
        }
    }
}
