using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpf.Core;
using twentySix.Framework.Core.Extensions;

namespace fita.core.Models
{
    public class HistoricalData : ICloneable
    {
        public LockableCollection<HistoricalDataPoint> Data { get; set; } = new();

        #region Add or update helper methods

        public void AddOrUpdate(DateTime key, decimal value)
        {
            Data.BeginUpdate();

            try
            {
                if (Data.All(x => x.Date.Date != key.Date))
                {
                    Data.Add(new HistoricalDataPoint(key, value));

                    SortData();
                }
                else
                {
                    Data.Single(x => x.Date.Date == key.Date).Value = value;
                }
            }
            finally
            {
                Data.EndUpdate();
            }
        }

        public void AddOrUpdate(Dictionary<DateTime, decimal> other)
        {
            Data.BeginUpdate();

            try
            {
                foreach (var (key, value) in other)
                {
                    if (Data.All(x => x.Date.Date != key.Date))
                    {
                        Data.Add(new HistoricalDataPoint(key, value));
                    }
                    else
                    {
                        Data.Single(x => x.Date.Date == key.Date).Value = value;
                    }
                }

                SortData();
            }
            finally
            {
                Data.EndUpdate();
            }
        }

        public void Delete(HistoricalDataPoint point)
        {
            Data.BeginUpdate();
            
            try
            {
                Data.Remove(point) ;
            }
            finally
            {
                Data.EndUpdate();
            }
        }

        private void SortData()
        {
            var tmpData = Data.ToList();
            Data.Clear();
            Data.AddRange(tmpData.OrderByDescending(x => x.Date));
        }

        #endregion

        public object Clone()
        {
            var obj = new HistoricalData
            {
                Data = new LockableCollection<HistoricalDataPoint>()
            };
            
            obj.Data.AddRange(Data);

            return obj;
        }

        public class HistoricalDataPoint
        {
            public HistoricalDataPoint()
            {
            }

            public HistoricalDataPoint(DateTime date, decimal value)
            {
                Date = date;
                Value = value;
            }
            
            public DateTime Date { get; set; }

            public decimal Value { get; set; }
        }
    }
}