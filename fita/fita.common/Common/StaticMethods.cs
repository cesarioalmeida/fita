using System;

namespace fita.core.Common
{
    public static class StaticMethods
    {
        public static decimal TransactionSignal(CategoryGroupEnum category, bool isCreditCard = false)
        {
            switch (category)
            {
                case CategoryGroupEnum.PersonalExpenses:
                case CategoryGroupEnum.BusinessExpenses:
                case CategoryGroupEnum.TransfersOut:
                    return !isCreditCard ? -1m : 1m;
                case CategoryGroupEnum.PersonalIncome:
                case CategoryGroupEnum.BusinessIncome:
                case CategoryGroupEnum.TransfersIn:
                    return !isCreditCard ? 1m : -1m;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }
    }
}