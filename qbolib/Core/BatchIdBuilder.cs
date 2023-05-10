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

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools.Core
{
    internal class BatchIdBuilder
    {
        private readonly string _prefix;
        protected BatchIdBuilder() { }

        public BatchIdBuilder(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentException("prefix argument is not defined.");
            }

            _prefix = prefix;
        }

        public string GetBatchId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id argument is not defined.");
            }

            return $"{_prefix}_{id.Replace(" ",string.Empty)}";
        }
        public string GetBatchGuid(Guid internalId)
        {
            if (internalId == Guid.Empty)
            {
                throw new ArgumentException("internalId argument should have positive value.");
            }

            return $"{_prefix}_{internalId}";
        }
    }
}
