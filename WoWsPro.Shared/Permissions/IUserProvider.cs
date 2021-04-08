using Microsoft.Extensions.DependencyInjection;
using WoWsPro.Shared.Models;

namespace WoWsPro.Shared.Permissions {
    public interface IUserProvider {
        bool IsLoggedIn { get; }
        Account Account { get; }
    }

    public static class IUserServiceProvider {
        public static IServiceCollection AddUserProvider<T> (this IServiceCollection services) where T : class, IUserProvider
            => services.AddScoped<IUserProvider, T>();
    }
}