using fita.ui.DTOs;
using twentySix.Framework.Core.UI.Models;

namespace fita.ui.Models
{
    public class Account : SynchronizableModelWithDTO<Account, AccountDTO>
    {
        public Account()
        {
            Currency = new Currency();
        }
        
        public string Name { get; set; }

        public Currency Currency { get; set; }
        
        public override AccountDTO GetDTO()
        {
            return new()
            {
                Id = Id,
                IsDeleted = IsDeleted,
                LastUpdated = LastUpdated,
                Name = Name,
                CurrencyId = Currency.Id
            };
        }

        public override bool PropertiesEqual(Account other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Name)
                   && other.Currency.Id == this.Currency.Id;
        }

        public override void SyncFrom(Account obj)
        {
            IsDeleted = obj.IsDeleted;
            LastUpdated = obj.LastUpdated;
            Name = (string) obj.Name?.Clone();
            Currency.SyncFrom(obj.Currency);
        }
    }
}