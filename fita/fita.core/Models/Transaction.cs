using System;
using System.Linq;
using fita.core.DTOs;
using LiteDB;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class Transaction //: SynchronizableModelWithDTO<Transaction, TransactionDTO>
    {
        public Transaction()
        {
            //Id = ObjectId.NewObjectId();
        }

        public DateTime Date { get; set; } = DateTime.Today;

        public string Payee { get; set; }

        public string Memo { get; set; }

        public string[] Tags { get; set; }

        public Category Category { get; set; } = new();

        public decimal Amount { get; set; }

        // public override TransactionDTO GetDTO()
        // {
        //     return new()
        //     {
        //         Id = Id,
        //         IsDeleted = IsDeleted,
        //         LastUpdated = LastUpdated,
        //         Date = Date,
        //         Payee = Payee,
        //         Memo = Memo,
        //         Tags = Tags,
        //         CategoryId = Category.Id,
        //         Amount = Amount
        //     };
        // }
        //
        // public override bool PropertiesEqual(Transaction other)
        // {
        //     if (other == null)
        //     {
        //         return false;
        //     }
        //
        //     return other.Date == Date
        //         && other.Payee.Equals(Payee)
        //         && other.Memo.Equals(Memo)
        //         && other.Tags == null && Tags == null || other.Tags != null && Tags != null &&
        //         other.Tags.SequenceEqual(Tags)
        //         && other.Category.Id == Category.Id
        //         && other.Amount == Amount;
        // }
        //
        // public override void SyncFrom(Transaction obj)
        // {
        //     IsDeleted = obj.IsDeleted;
        //     LastUpdated = obj.LastUpdated;
        //     Date = obj.Date;
        //     Payee = (string) obj.Payee?.Clone();
        //     Memo = (string) obj.Memo?.Clone();
        //     Tags = obj.Tags?.ToArray();
        //     Category.SyncFrom(obj.Category);
        //     Amount = obj.Amount;
        // }
    }
}