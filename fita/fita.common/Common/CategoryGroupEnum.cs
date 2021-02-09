using System.ComponentModel.DataAnnotations;

namespace fita.core.Common
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
        
        [Display(Name = "Business Expenses")]
        BusinessExpenses,
        
        [Display(Name = "Business Income")]
        BusinessIncome,
        
    }
}