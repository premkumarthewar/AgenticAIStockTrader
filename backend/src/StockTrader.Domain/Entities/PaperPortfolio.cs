namespace StockTrader.Domain.Entities;

public sealed class PaperPortfolio
{
    public Guid Id { get; set; }

    /// <summary>
    /// The owning user's id (ApplicationUser.Id). A plain Guid rather than a navigation
    /// property, so Domain has no dependency on Persistence's Identity types - exactly
    /// one portfolio per user is enforced by a unique index in PaperPortfolioConfiguration.
    /// </summary>
    public Guid UserId { get; set; }

    public decimal InitialCapital { get; set; }

    public decimal CashBalance { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime LastUpdatedOnUtc { get; set; }

    public ICollection<PaperPosition> Positions { get; set; } = [];

    public ICollection<PaperTrade> Trades { get; set; } = [];
}