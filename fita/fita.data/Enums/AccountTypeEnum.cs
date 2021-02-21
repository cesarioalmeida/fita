using System.ComponentModel.DataAnnotations;

namespace fita.data.Enums
{
    public enum AccountTypeEnum
    {
        [Display(Name = "Bank")]
        Bank,

        [Display(Name = "Credit Card")]
        CreditCard,

        [Display(Name = "Investment")]
        Investment,

        [Display(Name = "Asset")]
        Asset
    }
}