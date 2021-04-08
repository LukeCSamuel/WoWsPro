using Microsoft.Extensions.DependencyInjection;
using WoWsPro.Data.WarshipsApi;

namespace WoWsPro.Data.Operations
{
    public static class OperationsProvider
    {
        public static IServiceCollection AddOperations(this IServiceCollection services)
            => services
                .AddWarshipsApi()
                .AddScoped<AccountOperations>()
                .AddScoped<AdminAccountOperations>()
                .AddScoped<TournamentOperations>()
                .AddScoped<TeamOperations>();
    }
}