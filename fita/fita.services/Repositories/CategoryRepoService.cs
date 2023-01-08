using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Enums;
using fita.data.Models;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.services.Repositories;

[Export]
public class CategoryRepoService : RepositoryService<Category>, ISeedData
{
    public CategoryRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService) 
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.CategoryId);
        Collection.EnsureIndex(x => x.Name);
        Collection.EnsureIndex(x => x.Group);
    }

    public Task<Result> SeedData()
    {
        return Task.Run(async () =>
        {
            foreach (var category in DefaultCategories())
            {
                if (Collection.FindOne(x => x.Name == category.Name) is null)
                {
                    await Save(category);
                }
            }
                
            return Result.Success;
        });
    }

    private static IEnumerable<Category> DefaultCategories()
    {
        var personalIncome = new Dictionary<string, CategoryGroupEnum>
        {
            {"Salary", CategoryGroupEnum.PersonalIncome},
            {"Dividends", CategoryGroupEnum.PersonalIncome},
            {"Interest", CategoryGroupEnum.PersonalIncome},
            {"Capital Gains", CategoryGroupEnum.PersonalIncome},
            {"Gifts", CategoryGroupEnum.PersonalIncome},
            {"Other", CategoryGroupEnum.PersonalIncome}
        }.OrderBy(x => x.Key);

        var personalExpenses = new Dictionary<string, CategoryGroupEnum>
        {
            {"Groceries", CategoryGroupEnum.PersonalExpenses},
            {"Books", CategoryGroupEnum.PersonalExpenses},
            {"Dining", CategoryGroupEnum.PersonalExpenses},
            {"Entertainment", CategoryGroupEnum.PersonalExpenses},
            {"Gifts", CategoryGroupEnum.PersonalExpenses},
            {"Healthcare", CategoryGroupEnum.PersonalExpenses},
            {"Hobbies", CategoryGroupEnum.PersonalExpenses},
            {"Auto:Fuel", CategoryGroupEnum.PersonalExpenses},
            {"Auto:Parking", CategoryGroupEnum.PersonalExpenses},
            {"Auto:Repair & Maintenance", CategoryGroupEnum.PersonalExpenses},
            {"Auto:Toll Charges", CategoryGroupEnum.PersonalExpenses},
            {"Holiday:Lodging", CategoryGroupEnum.PersonalExpenses},
            {"Holiday:Travel", CategoryGroupEnum.PersonalExpenses},
            {"Home:Construction", CategoryGroupEnum.PersonalExpenses},
            {"Home:Furnishing", CategoryGroupEnum.PersonalExpenses},
            {"Home:Maintenance", CategoryGroupEnum.PersonalExpenses},
            {"Insurance:Auto", CategoryGroupEnum.PersonalExpenses},
            {"Insurance:Health", CategoryGroupEnum.PersonalExpenses},
            {"Insurance:House", CategoryGroupEnum.PersonalExpenses},
            {"Insurance:Life", CategoryGroupEnum.PersonalExpenses},
            {"Misc", CategoryGroupEnum.PersonalExpenses},
            {"Rent", CategoryGroupEnum.PersonalExpenses},
            {"Tax:Income Tax", CategoryGroupEnum.PersonalExpenses},
            {"Tax:National Insurance", CategoryGroupEnum.PersonalExpenses},
            {"Tax:Property Tax", CategoryGroupEnum.PersonalExpenses},
            {"Tax:Road Tax", CategoryGroupEnum.PersonalExpenses},
            {"Tax:Stamp Duty", CategoryGroupEnum.PersonalExpenses},
            {"Tax:Other Tax", CategoryGroupEnum.PersonalExpenses},
            {"Telecommunications", CategoryGroupEnum.PersonalExpenses},
            {"Utilities:Electricity", CategoryGroupEnum.PersonalExpenses},
            {"Utilities:Water", CategoryGroupEnum.PersonalExpenses},
            {"Bank:Service Charge", CategoryGroupEnum.PersonalExpenses},
            {"Bank:Interest", CategoryGroupEnum.PersonalExpenses},
            {"Clothes", CategoryGroupEnum.PersonalExpenses},
            {"Computer:Software", CategoryGroupEnum.PersonalExpenses},
            {"Computer:Hardware", CategoryGroupEnum.PersonalExpenses},
            {"Capital Loses", CategoryGroupEnum.PersonalExpenses}
        }.OrderBy(x => x.Key);

        var transfers = new Dictionary<string, CategoryGroupEnum>
        {
            {"Transfers In", CategoryGroupEnum.TransfersIn},
            {"Transfers Out", CategoryGroupEnum.TransfersOut}
        }.OrderBy(x => x.Key);
            
        var trades = new Dictionary<string, CategoryGroupEnum>
        {
            {"Trade Buy", CategoryGroupEnum.TradeBuy},
            {"Trade Sell", CategoryGroupEnum.TradeSell}
        }.OrderBy(x => x.Key);

        return personalIncome
            .Concat(personalExpenses)
            .Concat(transfers)
            .Concat(trades)
            .Select(x => new Category {Name = x.Key, Group = x.Value});
    }
}