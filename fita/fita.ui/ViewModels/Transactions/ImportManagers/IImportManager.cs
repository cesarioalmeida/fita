using System.Collections.Generic;
using fita.data.Models;

namespace fita.ui.ViewModels.Transactions.ImportManagers;

public interface IImportManager
{
    public IEnumerable<string> AppliesToAccountsWithName { get; }
    public string FileFilter { get; }
    
    public IEnumerable<Transaction> GetTransactions(string filePath, IReadOnlyList<Category> categories);
}