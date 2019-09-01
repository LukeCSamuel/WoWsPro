using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.Managers;
using WoWsPro.Data.Services;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;

namespace Tests
{
	public class App
	{

		DiscordManager GuildManager { get; }
		IWarshipsApi ShipsApi { get; }

		public App (IDiscordManager discordManager, IWarshipsApi shipsapi)
		{
			GuildManager = (DiscordManager)discordManager;
			ShipsApi = shipsapi;
		}

		public async Task RunAsync ()
		{
			try
			{
				foreach (var (id, name) in await ShipsApi.SearchPlayerAsync(Region.NA, "00d"))
				{
					var player = await ShipsApi.GetPlayerInfoAsync(Region.NA, id);
					Console.WriteLine($"{player.Nickname} created on {player.Created}.{(player.ClanId is long ? $"\r\n\tJoined {player.ClanId} on {player.JoinedClan}." : "")}");
				}
			}
			catch (Exception ex) when (ex is HttpRequestException || ex is WarshipsApiException)
			{
				Console.WriteLine(ex.Message);
			}

			//try
			//{
			//	var guild = await GuildManager.AddOrUpdateGuildAsync(new DiscordGuild()
			//	{
			//		GuildId = 1234,
			//		Name = "Best Girl Club",
			//		Icon = "http://pictures.rem.moe",
			//		Invite = "http://discord.gg/invite"
			//	});
			//	Console.WriteLine($"Operation succeeded. Account ID: {guild.Name}");
			//}
			//catch (UnauthorizedException ex)
			//{
			//	Console.WriteLine(ex.Message);
			//}
		}
	}
}
