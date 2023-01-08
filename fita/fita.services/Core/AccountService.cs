using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Core;

[Export(typeof(IAccountService))]
public class AccountService : IAccountService
{
    [Import]
    public ILoggingService LoggingService { get; set; }

    public Task<decimal> CalculateBalance(Account account, List<Transaction> transactions)
    {
        return Task.Run(
            () =>
            {
                var balance = 0m;

                if (account is null)
                {
                    return balance;
                }

                try
                {
                    balance += account.InitialBalance;

                    if (transactions is null)
                    {
                        return balance;
                    }

                    foreach (var transaction in transactions.OrderBy(x => x.Date))
                    {
                        balance += transaction.Deposit.GetValueOrDefault();
                        balance -= transaction.Payment.GetValueOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(CalculateBalance)}: {ex}");
                }

                return balance;
            });
    }

    public Task<decimal> CalculateBalance(Transaction transaction, decimal previousBalance)
    {
        return Task.Run(
            () =>
            {
                var balance = previousBalance;

                try
                {
                    if (transaction is null)
                    {
                        return balance;
                    }

                    balance += transaction.Deposit.GetValueOrDefault();
                    balance -= transaction.Payment.GetValueOrDefault();
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(CalculateBalance)}: {ex}");
                }

                return balance;
            });
    }
}