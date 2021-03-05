using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Enums;
using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class CategoryRepoService : RepositoryService<Category>, ISeedData
    {
        public CategoryRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public Task<Result> SeedData()
        {
            return Task.Run(async () =>
            {
                foreach (var category in DefaultCategories())
                {
                    if (Collection.FindOne(x => x.Name == category.Name) == null)
                    {
                        await SaveAsync(category);
                    };
                }
                
                return Result.Success;
            });
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.CategoryId);
            Collection.EnsureIndex(x => x.Name);
            Collection.EnsureIndex(x => x.Group);
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
                {"Computer:Hardware", CategoryGroupEnum.PersonalExpenses}
            }.OrderBy(x => x.Key);

            var businessIncome = new Dictionary<string, CategoryGroupEnum>
            {
                {"Sales", CategoryGroupEnum.BusinessIncome},
                {"Consulting", CategoryGroupEnum.BusinessIncome},
                {"Capital Gains", CategoryGroupEnum.BusinessIncome},
                {"Interest", CategoryGroupEnum.BusinessIncome},
                {"Other", CategoryGroupEnum.BusinessIncome}
            }.OrderBy(x => x.Key);

            var businessExpenses = new Dictionary<string, CategoryGroupEnum>
            {
                {"Advertising", CategoryGroupEnum.BusinessExpenses},
                {"Wages:Employees", CategoryGroupEnum.BusinessExpenses},
                {"Contract Labor", CategoryGroupEnum.BusinessExpenses},
                {"Legal", CategoryGroupEnum.BusinessExpenses},
                {"Travel:Dining", CategoryGroupEnum.BusinessExpenses},
                {"Travel:Entertainment", CategoryGroupEnum.BusinessExpenses},
                {"Travel:Lodging", CategoryGroupEnum.BusinessExpenses},
                {"Gifts", CategoryGroupEnum.BusinessExpenses},
                {"Repairs & Maintenance", CategoryGroupEnum.BusinessExpenses},
                {"Healthcare", CategoryGroupEnum.BusinessExpenses},
                {"Auto:Fuel", CategoryGroupEnum.BusinessExpenses},
                {"Auto:Parking", CategoryGroupEnum.BusinessExpenses},
                {"Auto:Repair & Maintenance", CategoryGroupEnum.BusinessExpenses},
                {"Auto:Toll Charges", CategoryGroupEnum.BusinessExpenses},
                {"Insurance:Other", CategoryGroupEnum.BusinessExpenses},
                {"Insurance:Health", CategoryGroupEnum.BusinessExpenses},
                {"Other", CategoryGroupEnum.BusinessExpenses},
                {"Rent & Lease", CategoryGroupEnum.BusinessExpenses},
                {"Bank:Service Charge", CategoryGroupEnum.BusinessExpenses},
                {"Bank:Interest", CategoryGroupEnum.BusinessExpenses},
                {"Taxes", CategoryGroupEnum.BusinessExpenses},
                {"Utilities", CategoryGroupEnum.BusinessExpenses},
                {"Supplies", CategoryGroupEnum.BusinessExpenses},
                {"IT:Software", CategoryGroupEnum.BusinessExpenses},
                {"IT:Hardware", CategoryGroupEnum.BusinessExpenses}
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
                .Concat(businessIncome)
                .Concat(businessExpenses)
                .Concat(transfers)
                .Concat(trades)
                .Select(x => new Category {Name = x.Key, Group = x.Value});
        }
    }
}