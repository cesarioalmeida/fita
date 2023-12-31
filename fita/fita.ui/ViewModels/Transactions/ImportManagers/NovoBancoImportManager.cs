using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using fita.data.Enums;
using fita.data.Models;

namespace fita.ui.ViewModels.Transactions.ImportManagers;

[Export(typeof(IImportManager))]
public class NovoBancoImportManager : IImportManager
{
    private const string Namespace = "urn:schemas-microsoft-com:office:spreadsheet";
    
    private static readonly Dictionary<string, string> DescriptionToCategoriesMapper = new()
    {
        // order matters here: imp selo should be before is comissao
        {"imp selo", "Tax:Stamp Duty"},
        {"is com. trf", "Tax:Stamp Duty"},
        {"imposto selo", "Tax:Stamp Duty"},
        {"comissao proc.", "Bank:Service Charge"},
        {"comissao servico", "Bank:Service Charge"},
        {"com. trf", "Bank:Service Charge"},
        {"manutencao de conta", "Bank:Service Charge"},
        {"edp", "Utilities:Electricity"},
        {"scribd", "Subscriptions"},
        {"www.freedom", "Subscriptions"},
        {"navigraph", "Subscriptions"},
        {"chatgpt", "Subscriptions"},
        {"jetbrains", "Subscriptions"}
    };

    public IEnumerable<string> AppliesToAccountsWithName => ["Novo Banco"];
    public string FileFilter => "Excel Files (.xls)|*.xls|XML Files (.xml)|*.xml|All Files (*.*)|*.*";

    public IEnumerable<(Transaction, bool)> GetTransactions(string filePath, IReadOnlyList<Category> categories)
    {
        // we expect the file path to be an excel file
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var xDoc = XDocument.Load(stream, LoadOptions.None);
        XNamespace ssNs = Namespace;

        var rows = xDoc.Descendants(ssNs + "Row").Skip(1);

        foreach (var row in rows)
        {
            var cells = row.Elements(ssNs + "Cell").Select(cell => cell.Element(ssNs + "Data")).ToList();

            if (cells.Count < 4) continue;
            var date = ParseDate(cells[0]);
            if (date == default) continue;
            var descriptionLower = cells[3].Value.ToLowerInvariant();

            yield return (new Transaction
            {
                Date = ParseDate(cells[0]),
                Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(descriptionLower),
                Deposit = ParseDecimal(cells[5]),
                Payment = -ParseDecimal(cells[4]),
                Category = MapCategory(descriptionLower, categories)
            }, true);
        }
    }

    private static DateTime ParseDate(XElement element)
        => DateTime.TryParse(element.Value, out var date) ? date : default;

    private static decimal? ParseDecimal(XElement element)
        => decimal.TryParse(element.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    // map partial strings in description to categories
    private static Category MapCategory(string description, IEnumerable<Category> categories)
        => DescriptionToCategoriesMapper
            .Where(x => description.Contains(x.Key))
            .Select(x => categories.First(y => y.Name.Contains(x.Value)))
            .FirstOrDefault() ?? categories.First(x => x.Name.Contains("Misc") && x.Group == CategoryGroupEnum.PersonalExpenses);
}