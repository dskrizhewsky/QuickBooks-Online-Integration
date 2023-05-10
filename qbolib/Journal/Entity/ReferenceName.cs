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

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.Entity
{
    public class ReferenceName : IEquatable<ReferenceName>, IEqualityComparer<ReferenceName>
    {
        private string _displayName;
        protected ReferenceName() { }
        public ReferenceName(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Incorrect  ID argument.");
            }

            Id = id;
        }

        private char[] GetIllegalChars()
        {
            return new []{ ':', '\t', '\n' };
        }


        public string Id { get; protected set; }

        public string DisplayName
        {
            get => _displayName;
            protected set
            {
                VerifyIllegalChars(value);
                _displayName = value;
            }
        }

        private void VerifyIllegalChars(string value)
        {
            if (value.IndexOfAny(GetIllegalChars()) != -1)
            {
                throw new Exception("DisplayName contains illegal character.");
            }
        }

        public string Phone { get; protected set; }
        public bool Equals(ReferenceName other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (this.Id == other.Id)
            {
                return true;
            }

            return false;
        }

        public bool Equals(ReferenceName x, ReferenceName y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x.Id == y.Id)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(ReferenceName obj)
        {
            int hashId = obj.Id == null ? 0 : obj.Id.GetHashCode();

            return hashId;
        }

        public override int GetHashCode()
        {
            int hashId = Id == null ? 0 : Id.GetHashCode();

            return hashId;
        }
    }

    internal class EmptyReferenceName : ReferenceName
    {

    }
}
