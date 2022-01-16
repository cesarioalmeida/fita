using System.ComponentModel.DataAnnotations;

namespace fita.data.Enums
{
    public enum CategoryGroupEnum
    {
        [Display(Name = "Personal Expenses")]
        PersonalExpenses,
        
        [Display(Name = "Personal Income")]
        PersonalIncome,
        
        [Display(Name = "Transfers In")]
        TransfersIn,
        
        [Display(Name = "Transfers Out")]
        TransfersOut,
        
        [Display(Name = "Trade Buy")]
        TradeBuy,
        
        [Display(Name = "Trade Sell")]
        TradeSell,
    }
}