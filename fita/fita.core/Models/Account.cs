using fita.core.DTOs;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class Account : SynchronizableModelWithDTO<Account, AccountDTO>
    {
        public Account()
        {
            Currency = new Currency();
        }
        
        public string Name { get; set; }

        public Currency Currency { get; set; }
        
        public bool IsCreditCard { get; set; }
        
        public override AccountDTO GetDTO()
        {
            return new()
            {
                Id = Id,
                IsDeleted = IsDeleted,
                LastUpdated = LastUpdated,
                Name = Name,
                CurrencyId = Currency.Id,
                IsCreditCard = IsCreditCard
            };
        }

        public override bool PropertiesEqual(Account other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Name)
                   && other.Currency.Id == Currency.Id
                   && other.IsCreditCard == IsCreditCard;
        }

        public override void SyncFrom(Account obj)
        {
            IsDeleted = obj.IsDeleted;
            LastUpdated = obj.LastUpdated;
            Name = (string) obj.Name?.Clone();
            Currency.SyncFrom(obj.Currency);
            IsCreditCard = obj.IsCreditCard;
        }
    }
}