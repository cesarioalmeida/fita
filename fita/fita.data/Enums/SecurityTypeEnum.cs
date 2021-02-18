using System.ComponentModel.DataAnnotations;

namespace fita.data.Enums
{
    public enum SecurityTypeEnum
    {
        [Display(Name = "Stock")]
        Stock,

        [Display(Name = "Bond")]
        Bond,

        [Display(Name = "Commodity")]
        Commodity,

        [Display(Name = "Mutual Fund")]
        MutualFund,

        [Display(Name = "Real Estate")]
        RealEstate
    }
}