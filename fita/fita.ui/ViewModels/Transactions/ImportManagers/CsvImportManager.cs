using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using fita.data.Enums;
using fita.data.Models;

namespace fita.ui.ViewModels.Transactions.ImportManagers;

[Export(typeof(IImportManager))]
public class CsvImportManager : IImportManager
{
    private static readonly Dictionary<string, string> DescriptionToCategoriesMapper = new()
    {
        // order matters
        {"coop", "Groceries"},
        {"migros", "Groceries"},
        {"lidl", "Groceries"},
        {"nishi", "Groceries"},
        {"asia store", "Groceries"},
        {"ingrid", "Rent"},
        {"sunrise", "Telecommunications"},
        {"swisscom", "Telecommunications"},
        {"toppharm", "Healthcare"},
        {"sanitas", "Insurance:Health"},
        {"leonteq", "Salary"},
        {"sbb", "Commuting"},
        {"cembrapay", "Commuting"},
        {"manora", "Dining"},
        {"manor", "Groceries"},
        {"ubs rest.", "Dining"},
        {"migrolino", "Dining"},
        {"twitch", "Subscriptions"},
        {"netflix", "Subscriptions"},
        {"github", "Subscriptions"},
        {"booking.com", "Holiday:Travel"},
        {"swiss international air", "Holiday:Travel"},
        {"sprüngli", "Dining"},
    };

    private static readonly List<string> DescriptionsToDeSelect = ["viseca", "bancomat"];
    
    private static readonly Dictionary<string, string> PortionsToReplace = new()
    {
        // order matters
        {"einkauf twint,", ""},
        {"einkauf", ""},
        {"e-banking", ""},
    };

    public IEnumerable<string> AppliesToAccountsWithName => ["Raiffeisen", "One", "Novo Banco Gold"];

    public string FileFilter => "CSV Files (.csv)|*.csv|All Files (*.*)|*.*";

    public IEnumerable<(Transaction, bool)> GetTransactions(string filePath, IReadOnlyList<Category> categories)
    {
        // we expect the file path to be a csv file
        // in the following format:
        //IBAN;Date;Description;Credit/Debit Amount;Balance;Value Date
        var lines = System.IO.File.ReadAllLines(filePath);
        // skip the first line
        lines = lines.Skip(1).ToArray();

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 4) continue;

            var descriptionLower = parts[2].ToLowerInvariant();
            var amount = ParseDecimal(parts[3]);
            
            foreach (var (key, value) in PortionsToReplace)
            {
                descriptionLower = descriptionLower.Replace(key, value).Trim();
            }

            yield return (new Transaction
            {
                Date = ParseDate(parts[1]),
                Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(descriptionLower),
                Deposit = amount > 0 ? amount : null,
                Payment = amount < 0 ? -amount : null,
                Category = MapCategory(descriptionLower, categories)
            }, !DescriptionsToDeSelect.Any(x => descriptionLower.Contains(x)));
        }
    }

    private static DateTime ParseDate(string element)
        => DateTime.TryParse(element, out var date) ? date : default;

    private static decimal? ParseDecimal(string element)
        => decimal.TryParse(element, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    private static Category MapCategory(string description, IEnumerable<Category> categories)
        => DescriptionToCategoriesMapper
               .Where(x => description.Contains(x.Key))
               .Select(x => categories.First(y => y.Name.Contains(x.Value)))
               .FirstOrDefault() ??
           categories.First(x => x.Name.Contains("Misc") && x.Group == CategoryGroupEnum.PersonalExpenses);
}