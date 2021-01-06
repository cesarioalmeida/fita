using System;
using System.Collections.Generic;
using System.Linq;
using fita.core.DTOs;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class HistoricalData : SynchronizableModelWithDTO<HistoricalData, HistoricalDataDTO>
    {
        public Dictionary<DateTime, decimal> Data { get; private set; } = new Dictionary<DateTime, decimal>();

        public override HistoricalDataDTO GetDTO()
        {
            return new()
            {
                Id = Id,
                IsDeleted = IsDeleted,
                LastUpdated = LastUpdated,
                Data = Data.ToDictionary(x => x.Key, x => x.Value)
            };
        }

        public override bool PropertiesEqual(HistoricalData other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Data == null && Data == null ||
                   other.Data != null && Data != null && other.Data.SequenceEqual(Data);
        }

        public override void SyncFrom(HistoricalData obj)
        {
            IsDeleted = obj.IsDeleted;
            LastUpdated = obj.LastUpdated;
            Data = obj.Data.ToDictionary(x => x.Key, x => x.Value);
        }

        #region Add or update helper methods

        public void AddOrUpdate(DateTime key, decimal value)
        {
            if (!Data.ContainsKey(key))
            {
                Data.Add(key, value);

                SortData();
            }
            else
            {
                Data[key] = value;
            }
        }

        public void AddOrUpdate(Dictionary<DateTime, decimal> other)
        {
            foreach (var (key, value) in other)
            {
                if (!Data.ContainsKey(key))
                {
                    Data.Add(key, value);
                }
                else
                {
                    Data[key] = value;
                }
            }

            SortData();
        }

        private void SortData()
        {
            Data = new Dictionary<DateTime, decimal>(Data.OrderByDescending(x => x.Key));
        }

        #endregion
    }
}