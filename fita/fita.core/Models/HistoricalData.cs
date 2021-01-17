using System;
using System.Collections.Generic;
using System.Linq;

namespace fita.core.Models
{
    public class HistoricalData : ICloneable
    {
        public int HistoricalDataId { get; set; }
        
        public Dictionary<DateTime, decimal> Data { get; private set; } = new();

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

        public object Clone()
        {
            return new HistoricalData
            {
                Data = Data?.ToDictionary(x => x.Key, x => x.Value)
            };
        }
    }
}