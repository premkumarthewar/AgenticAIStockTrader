using Microsoft.AspNetCore.Identity;

namespace StockTrader.Persistence.Identity;

/// <summary>
/// The app's user record backing ASP.NET Core Identity. Kept minimal - DisplayName and
/// CreatedOnUtc are the only additions over IdentityUser&lt;Guid&gt; - since everything
/// else (paper portfolio ownership, trade approvals) references users by Id alone rather
/// than by navigation property, so Domain stays free of any Identity/Persistence
/// dependency: PaperPortfolio.UserId is a plain Guid with no FK navigation to this type.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}
