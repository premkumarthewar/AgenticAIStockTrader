using System;
using System.Collections.Generic;
using System.Text;

namespace StockTrader.Application.Common.Interfaces;

public interface IMemorySummarizer
{
    Task RefreshSummaryAsync(string symbol, CancellationToken cancellationToken = default);
}
