using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;

namespace fita.services.Core;

public interface IAccountService
{
    Task<decimal> CalculateBalance(Account account, List<Transaction> transactions);

    Task<decimal> CalculateBalance(Transaction transaction, decimal previousBalance);
}