using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Client.Utils;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Client.Services
{
    public interface ITournamentService
    {
        long CurrentTournamentId { get; set; }
        Region WaitListRegion { get; set; }
        Cache<Tournament> Current { get; }
        Cache<List<Tournament>> Previews { get; }
        Cache<List<TournamentTeam>> CurrentTeams { get; }
        Cache<List<long>> WaitList { get; }
        Cache<List<TournamentTeam>> UserTeams { get; }

    }

    public class TournamentService : ITournamentService, IDisposable
    {
        private long _currentTournamentId;
        public long CurrentTournamentId
        {
            get => _currentTournamentId;
            set
            {
                if (value != _currentTournamentId)
                {
                    _currentTournamentId = value;
                    Current.Invalidate();
                    CurrentTeams.Invalidate();
                    UserTeams.Invalidate();
                }
            }
        }
        private Region _waitlistRegion;
        public Region WaitListRegion
        {
            get => _waitlistRegion;
            set
            {
                if (_waitlistRegion != value)
                {
                    _waitlistRegion = value;
                    WaitList.Invalidate();
                }
            }
        }

        public Cache<Tournament> Current { get; }
        public Cache<List<Tournament>> Previews { get; }
        public Cache<List<TournamentTeam>> CurrentTeams { get; }
        public Cache<List<long>> WaitList { get; }
        public Cache<List<TournamentTeam>> UserTeams { get; }


        HttpClient Http { get; }
        IUserService UserService { get; }

        public TournamentService(HttpClient http, IUserService userService)
        {
            Http = http;
            UserService = userService;
            Previews = new Cache<List<Tournament>>(() => Http.GetAsAsync<List<Tournament>>($"api/Tournament"));
            Current = new Cache<Tournament>(GetTournament);
            CurrentTeams = new Cache<List<TournamentTeam>>(GetCurrentTeams);
            WaitList = new Cache<List<long>>(GetWaitListAsync);
            UserTeams = new Cache<List<TournamentTeam>>(GetUserTeams);
            UserService.User.Updated += OnUserUpdated;
        }

        private void OnUserUpdated(object sender, User user)
        {
            UserTeams.Invalidate();
        }

        public async Task<Tournament> GetTournament()
        {
            try
            {
                if (CurrentTournamentId == 0)
                {
                    return null;
                }

                return await Http.GetAsAsync<Tournament>($"api/Tournament/{CurrentTournamentId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<TournamentTeam>> GetCurrentTeams()
        {
            try
            {
                if (CurrentTournamentId == 0)
                {
                    return null;
                }

                return await Http.GetAsAsync<List<TournamentTeam>>($"api/Tournament/{CurrentTournamentId}/teams");
            }
            catch
            {
                return null;
            }
        }

        private async Task<List<long>> GetWaitListAsync()
        {
            try
            {
                if (CurrentTournamentId == 0)
                {
                    return new List<long>();
                }

                return await Http.GetAsAsync<List<long>>($"api/Tournament/{CurrentTournamentId}/{WaitListRegion}/waitlist");
            }
            catch
            {
                return new List<long>();
            }
        }

        private async Task<List<TournamentTeam>> GetUserTeams()
        {
            try
            {
                if (!(await UserService.User).IsLoggedIn)
                {
                    return new List<TournamentTeam>();
                }

                return await Http.GetAsAsync<List<TournamentTeam>>($"api/Team/me/{CurrentTournamentId}");
            }
            catch
            {
                return new List<TournamentTeam>();
            }
        }

        public void Dispose()
        {
            UserService.User.Updated -= OnUserUpdated;
        }
    }

    public static class TournamentServiceProvider
    {
        public static IServiceCollection AddTournamentService(this IServiceCollection services)
            => services.AddScoped<ITournamentService, TournamentService>();
    }
}
