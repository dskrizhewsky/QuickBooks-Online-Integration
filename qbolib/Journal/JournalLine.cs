using System;
using Automate.ExternalAccountingTools.Journal.Entity;

namespace Automate.ExternalAccountingTools.Journal
{
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
public class JournalLine 
    {
        public JournalLine()
        {
            Amount = new decimal(0.0);
        }
        public JournalLine(Account account, ReferenceName name, Location location, decimal amount) : this()
        {
            if (account == null)
            {
                throw new ArgumentNullException("account argument should not be null.");
            }

            if ((account.IsPayable || account.IsReceivable) && name == null)
            {
                throw new BusinessRuleException("Account Payable and Account Rceivable requires name argument to be defined.");
            }

            if (account.IsPayable && !(name is Vendor))
            {
                throw new BusinessRuleException("Account Payable requires vendor to be defined.");
            }

            if (account.IsReceivable && !(name is Customer))
            {
                throw new BusinessRuleException("Account Receivable requires customer to be defined.");
            }

            Account = account;
            Name = name ?? new EmptyReferenceName();

            Location = location;
            Amount = amount;
        }

        public Account Account { get; }
        public ReferenceName Name { get; }
        public Location Location { get; }
        public decimal Amount { get; private set; }

        public virtual bool IsDebitAccount()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsCreditAccount()
        {
            throw new NotImplementedException();
        }

        public JournalLine FindOrAdd(string name)
        {
            throw new NotImplementedException();
        }


    }

    public class DebitLine : JournalLine
    {
        public override bool IsDebitAccount() => true;

        public override bool IsCreditAccount() => !IsCreditAccount();

        public DebitLine(Account account, ReferenceName name, Location location, decimal amount) : base(account, name, location, amount)
        {
        }
    }

    public class CreditLine : JournalLine
    {
        public override bool IsDebitAccount() => !IsCreditAccount();

        public override bool IsCreditAccount() => true;

        public CreditLine(Account account, ReferenceName name, Location location, decimal amount) : base(account, name, location, amount)
        {
        }
    }
}
