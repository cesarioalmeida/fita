using fita.data.Enums;
using fita.data.Models;

namespace fita.data.Extensions;

public static class CategoryExtensions
{
    public static bool IsCapitalGains(this Category item) =>
        item.Group == CategoryGroupEnum.PersonalIncome && item.Name == "Capital Gains";
    
    public static bool IsCapitalLoses(this Category item) =>
        item.Group == CategoryGroupEnum.PersonalExpenses && item.Name == "Capital Loses";
}