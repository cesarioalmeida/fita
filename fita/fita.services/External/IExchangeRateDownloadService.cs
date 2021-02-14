﻿using System.Threading.Tasks;
using fita.data.Models;

namespace fita.services.External
{
    public interface IExchangeRateDownloadService
    {
        Task<Result> UpdateAllAsync();

        Task<Result> UpdateAsync(ExchangeRate exchangeRate);
    }
}