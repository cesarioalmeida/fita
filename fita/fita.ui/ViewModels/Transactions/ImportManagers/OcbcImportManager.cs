using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using fita.data.Enums;
using fita.data.Models;

namespace fita.ui.ViewModels.Transactions.ImportManagers;

[Export(typeof(IImportManager))]
public class OcbcImportManager : IImportManager
{
    private static readonly Dictionary<string, string> DescriptionToCategoriesMapper = new()
    {
        // order matters
        {"cash rebate", "Interest"}
    };

    private static readonly List<string> DescriptionsToDeSelect = ["payment by internet"];

    public IEnumerable<string> AppliesToAccountsWithName => ["OCBC", "OCBC 365"];

    public string FileFilter => "CSV Files (.csv)|*.csv|All Files (*.*)|*.*";

    public IEnumerable<(Transaction, bool)> GetTransactions(string filePath, IReadOnlyList<Category> categories)
    {
        // we expect the file path to be a csv file
        // in the following format:
        //Transaction date,Description,Withdrawals (SGD),Deposits (SGD)
        var lines = System.IO.File.ReadAllLines(filePath);
        // skip the first 7 lines
        lines = lines.Skip(7).ToArray();

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length < 4) continue;

            var descriptionLower = parts[1].ToLowerInvariant();
            var amount = -ParseDecimal(parts[2]) ?? ParseDecimal(parts[3]);

            yield return (new Transaction
            {
                Date = ParseDate(parts[0]),
                Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(descriptionLower),
                Deposit = amount > 0 ? amount : null,
                Payment = amount < 0 ? -amount : null,
                Category = MapCategory(descriptionLower, categories)
            }, !DescriptionsToDeSelect.Any(x => descriptionLower.Contains(x)));
        }
    }

    private static DateTime ParseDate(string element)
        => DateTime.TryParseExact(element, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
            out var date)
            ? date
            : default;

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