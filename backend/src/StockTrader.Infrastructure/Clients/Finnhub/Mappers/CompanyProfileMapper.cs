using StockTrader.Application.MarketData.Dtos;
using StockTrader.Infrastructure.Clients.Finnhub.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockTrader.Infrastructure.Clients.Finnhub.Mappers
{
    internal static class CompanyProfileMapper
    {
        public static CompanyProfileDto Map(
        FinnhubCompanyProfileResponse source)
        {
            return new CompanyProfileDto
            {
                Symbol = source.Ticker,
                CompanyName = source.Name,
                Country = source.Country,
                Currency = source.Currency,
                Exchange = source.Exchange,
                Industry = source.Industry,
                Website = source.Website,
                MarketCapitalization = source.MarketCapitalization,
                Description = string.Empty
            };
        }
    }
}
