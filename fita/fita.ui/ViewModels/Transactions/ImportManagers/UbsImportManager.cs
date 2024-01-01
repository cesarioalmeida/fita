using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using fita.data.Enums;
using fita.data.Models;

namespace fita.ui.ViewModels.Transactions.ImportManagers;

[Export(typeof(IImportManager))]
public class UbsImportManager : IImportManager
{
    private static readonly Dictionary<string, string> DescriptionToCategoriesMapper = new()
    {
        // order matters
        {"cembrapay", "Commuting"},
        {"interest calculation", "Interest"},
        {"hsgutschrift", "Salary"},
    };

    private static readonly List<string> DescriptionToDeSelectMapper = ["incoming sic-payment"];

    public IEnumerable<string> AppliesToAccountsWithName => ["UBS", "UBS Gold"];

    public string FileFilter => "CSV Files (.csv)|*.csv|All Files (*.*)|*.*";

    public IEnumerable<(Transaction, bool)> GetTransactions(string filePath, IReadOnlyList<Category> categories)
    {
        // we expect the file path to be a csv file
        // in the following format:
        //Trade date;Trade time;Booking date;Value date;Currency;Debit;Credit;Individual amount;Balance;Transaction no.;Description1;Description2;Description3;Footnotes;
        var lines = System.IO.File.ReadAllLines(filePath);
        // skip the first 10 lines
        lines = lines.Skip(10).ToArray();

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 13) continue;

            var description1Lower = parts[10].ToLowerInvariant().Replace("\"", "");
            var description2Lower = parts[11].ToLowerInvariant().Replace("\"", "");
            var descriptionLower = description1Lower != "---" ? description1Lower : description2Lower;
            var amount = ParseDecimal(parts[5]) ?? ParseDecimal(parts[6]);
            
            if(amount is null or 0m) continue;

            yield return (new Transaction
            {
                Date = ParseDate(parts[0]),
                Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(descriptionLower),
                Deposit = amount > 0 ? amount : null,
                Payment = amount < 0 ? -amount : null,
                Category = MapCategory(descriptionLower, categories)
            }, !DescriptionToDeSelectMapper.Any(x => descriptionLower.Contains(x)));
        }
    }

    private static DateTime ParseDate(string element)
        => DateTime.TryParseExact(element, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
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