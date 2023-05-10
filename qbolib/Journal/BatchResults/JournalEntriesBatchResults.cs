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

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools.Journal.BatchResults
{
    public class JournalEntriesBatchResults
    {
        private readonly List<JournalEntry> _inconsistent = new List<JournalEntry>();
        private readonly List<JournalEntry> _failed = new List<JournalEntry>();
        private readonly List<JournalEntry> _added = new List<JournalEntry>();
        private readonly Dictionary<string, string> _errorDescriptions = new Dictionary<string, string>();

        private JournalEntriesBatchResults() { }

        public JournalEntriesBatchResults(VerificationResults verification)
        {
            PreVerification = verification;
        }

        public VerificationResults PreVerification { get; protected set; }

        public void AddFailed(JournalEntry entry, string errorDescription)
        {
            VerifyEntryArgument(entry);
            if (errorDescription == null)
            {
                throw new ArgumentNullException("errorDescription argument should not be null");
            }

            _failed.Add(entry);
            _errorDescriptions.Add(entry.GetBatchId(), errorDescription);
        }

        private void VerifyEntryArgument(JournalEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry argument should not be null");
            }
        }

        private void VerifyEntryArgument(Intuit.Ipp.Data.JournalEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry argument should not be null");
            }
        }

        public string GetErrorDescription(JournalEntry entry)
        {
            VerifyEntryArgument(entry);
            _errorDescriptions.TryGetValue(entry.GetBatchId(), out string errorDescription);
            return errorDescription;
        }

        public void AddInconsistent(JournalEntry entry)
        {
            VerifyEntryArgument(entry);
            _inconsistent.Add(entry);
        }

        public void AddAdded(JournalEntry entry)
        {
            VerifyEntryArgument(entry);
            _added.Add(entry);
        }

        public ImmutableList<JournalEntry> GetInconsistent()
        {
            return _inconsistent.ToImmutableList();
        }

        public ImmutableList<JournalEntry> GetFailed()
        {
            return _failed.ToImmutableList();
        }


        public ImmutableList<JournalEntry> GetAdded()
        {
            return _added.ToImmutableList();
        }

        public bool AllPassed()
        {
            return !GetInconsistent().Any() && !GetFailed().Any() && GetAdded().Any();
        }

    }
}