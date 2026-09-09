using System;
using System.Collections.Generic;
using System.Text;

namespace StockTrader.Application.AI.Dtos;

public sealed record WatchlistAnalysisDto
{
    public IReadOnlyList<AlertDto> Alerts { get; init; } = [];


    public IReadOnlyList<string> Recommendations { get; init; } = [];
}
