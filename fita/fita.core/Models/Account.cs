using System;
using System.Collections.Generic;
using System.Linq;
using fita.core.Common;
using fita.core.DTOs;
using LiteDB;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class Account : SynchronizableModelWithDTO<Account, AccountDTO>
    {
        public Account()
        {
            Id = ObjectId.NewObjectId();
            Currency = new Currency();
        }

        public string Group { get; set; }

        public string Name { get; set; }

        public Currency Currency { get; set; }

        public bool IsCreditCard { get; set; }

        public SortedList<DateTime, Transaction> Transactions { get; set; } = new();

        public decimal CurrentBalance =>
            Transactions.Sum(
                x => StaticMethods.TransactionSignal(x.Value.Category.Group, IsCreditCard) * x.Value.Amount);

        public override AccountDTO GetDTO()
        {
            return new()
            {
                Id = Id,
                IsDeleted = IsDeleted,
                LastUpdated = LastUpdated,
                Group = Group,
                Name = Name,
                //CurrencyId = Currency.Id,
                IsCreditCard = IsCreditCard,
                TransactionsIds = Transactions?.Select(x => x.Value.Id).ToArray()
            };
        }

        public override bool PropertiesEqual(Account other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Group.Equals(Group)
                   && other.Name.Equals(Name)
                   //&& other.Currency.Id == Currency.Id
                   && other.IsCreditCard == IsCreditCard
                   && other.Transactions == null && Transactions == null ||
                   other.Transactions != null && Transactions != null && other.Transactions.SequenceEqual(Transactions);
        }

        public override void SyncFrom(Account obj)
        {
            IsDeleted = obj.IsDeleted;
            LastUpdated = obj.LastUpdated;
            Group = (string) obj.Group?.Clone();
            Name = (string) obj.Name?.Clone();
            //Currency.SyncFrom(obj.Currency);
            IsCreditCard = obj.IsCreditCard;
            Transactions =
                new SortedList<DateTime, Transaction>(obj?.Transactions.ToDictionary(x => x.Key, x => x.Value));
        }
    }
}