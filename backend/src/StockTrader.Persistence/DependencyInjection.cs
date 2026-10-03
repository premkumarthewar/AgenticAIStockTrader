using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockTrader.Application.Approvals.Interfaces;
using StockTrader.Application.Auth.Interfaces;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Application.Watchlist.Interfaces;
using StockTrader.Persistence.Context;
using StockTrader.Persistence.Identity;
using StockTrader.Persistence.Options;
using StockTrader.Persistence.Services;

namespace StockTrader.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StockTraderDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // ASP.NET Core Identity, EF-backed via the same StockTraderDbContext (now an
        // IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>). AddIdentityCore
        // rather than AddIdentity because there's no cookie/UI login here - only
        // UserManager, used by AuthService to back the JWT-issuing register/login
        // endpoints; AddSignInManager is added on top for CheckPasswordSignInAsync-style
        // lockout-aware password checks.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<StockTraderDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IMemoryService, MemoryService>();

        services.AddScoped<IPaperTradingPersistence, PaperTradingPersistenceService>();

        services.AddScoped<IWatchlistService, WatchlistService>();

        services.AddScoped<ITradeApprovalPersistence, TradeApprovalPersistenceService>();

        return services;
    }
}
