using System;
using System.Collections.Generic;
using Bookkeeper.Accounting;
using Bookkeeper.Infrastructure.Interfaces;

namespace Bookkeeper {
    public interface ILedger {
        ITrialBalance GetTrialBalance();
        IEnumerable<IAccount> Accounts { get; }
        string Name { get; }
        void AddAccount(int accountNo, string accountName, AccountType accountType);
        void RecordTransaction(int accountNumber, DateTime transactionDate, string transactionReference, decimal amount);
    }
}