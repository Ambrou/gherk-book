﻿using System;
using System.Linq;
using Bookkeeper;
using Bookkeeper.Accounting;
using Bookkeeper.Infrastructure;
using Bookkeeper.Infrastructure.Interfaces;
using SharpTestsEx;
using TechTalk.SpecFlow;

namespace TestBookkeeper {
    [Binding]
    public class LedgerStepDefinitions {

        [BeforeScenario]
        public void CreateBusiness() {
            var business = Business.Create();
            ScenarioContext.Current.Add("business", business);
        }

        [Given(@"a (.*) ledger with id (\d+) and a (.*) account no. (\d+) as controlling account")]
        public void GivenASubledger(string ledgerName, int ledgerId, AccountType controllingAccountType, int controllingAccountNumber) {
            var business = (IBusiness) ScenarioContext.Current["business"];
            var ledger = Ledger.CreateLedger(ledgerId, ledgerName);
            business.Add<ILedger>(ledger);
        }

        [Given(@"[a|an] (asset|liability|revenue|expense|equity) account (\d+) ""(.*)"" in (.*)")]
        public void CreateNewAccount(AccountType accountType, int accountNo, string accountName, string ledgerName) {
            var business = (IBusiness)ScenarioContext.Current["business"];
            var ledger = business.Find<ILedger>(ledgerName);
            ledger.AddAccount(accountNo, accountName, accountType);
        }

        [When(@"I record the following transactio[n|ns] in the (.*) ledger:")]
        public void WhenIRecordTheFollowingTransaction(string ledger, Table transactions) {
            RecordIn(ledger, transactions);
        }

        private static void RecordIn(string ledgerName, Table transactions) {
            var business = (IBusiness)ScenarioContext.Current["business"];
            var ledger = business.Find<ILedger>(ledgerName);
            foreach(var transaction in transactions.Rows.ToList()) {
                var accountNumber = Convert.ToInt32(transaction["AccountNumber"]);
                var date = Convert.ToDateTime(transaction["Date"]);
                var transactionReference = transaction["TransactionReference"];
                var amount = Convert.ToDecimal(transaction["Amount"]);
                var accountName = transaction["AccountName"];
                Console.WriteLine(String.Format("Acct: {0} - {1}, {2} - {3}, ${4}", accountName, accountNumber, date, transactionReference, amount));
                ledger.RecordTransaction(accountNumber, date, transactionReference,
                                         amount);
            }
        }

        [Then(@"the (.*) ledger should (not balance|balance)\.")]
        public void TrialBalanceOf(string ledgerName, string shouldBalanceOrNot)
        {
            var business = (IBusiness)ScenarioContext.Current["business"];
            var ledger = business.Find<ILedger>(ledgerName);
            var trialBalance = ledger.GetTrialBalance();
            if(shouldBalanceOrNot == "balance") {
                trialBalance.IsBalanced.Should().Be(true);
            } else {
                trialBalance.IsBalanced.Should().Be(false); 
            }

            var reportWriter = Ioc.Resolve<IPrintLedgerReports>();
            reportWriter.For = ledger;
            foreach (var account in ledger.Accounts) {
                reportWriter.Print<IAccount>(account.AccountNumber);
            }
        }
    }
}