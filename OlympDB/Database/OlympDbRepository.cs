using Bogus;
using Faker;
using Microsoft.EntityFrameworkCore;
using OlympDB.Classes;
using System.Globalization;
using Country = OlympDB.Classes.Country;

namespace OlympDB.Database
{
	public class OlympDbRepository
	{
		private static List<Country> countries = new();
		private static List<Olympiс> olympics = new();
		private static List<Player> players = new();
		private static List<Event> events = new();
		private static List<Result> results = new();
		private OlympDbContext context;

		public DbSet<Country> Countries => context.Countries;
		public DbSet<Olympiс> Olympics => context.Olympics;
		public DbSet<Player> Players => context.Players;
		public DbSet<Event> Events => context.Events;
		public DbSet<Result> Results => context.Results;

		public OlympDbRepository(OlympDbContext context)
		{
			this.context = context;
		}


		#region Tasks

		/// <summary>
		/// Task 1
		/// Для Олимпийских игр заданного года сгенерируйте список (год рождения, количество игроков, количество золотых медалей), 
		/// содержащий годы, в которые родились игроки, количество игроков, родившихся в каждый из этих лет, которые выиграли 
		/// по крайней мере одну золотую медаль, и количество золотых медалей, завоеванных игроками, родившимися в этом году
		/// </summary>
		/// <param name="year">Year for olympic games</param>
		/// <exception cref="ArgumentException">Олимпиады с заданным годом проведения не найдены</exception>
		public object GetOlympicPlayersListAtYear(int year)
		{
			if (!context.Olympics.Any(x => x.Year == year))
				throw new ArgumentException("No Olympic games at such year");

			var query = context.Olympics.Where(x => x.Year == year).Take(1)
				.Select(x => new
				{
					OlympicYear = year,
					Years = x.Events
						.SelectMany(ev => ev.Results.Select(x => x.Player.BirthDate.Year))
						.GroupBy(yr => yr)
						.Select(g => g.First()),
					AmountOfBornPlayers = x.Events
						.SelectMany(ev => ev.Results.Select(x => x.Player.BirthDate.Year))
						.GroupBy(yr => yr)
						.Select(g => context.Players
							.Where(p => p.BirthDate.Year == g.First() &&
									context.Results.FirstOrDefault(r => r.PlayerId == p.PlayerId && r.Medal == "Gold") != null)
							.Count()),
					AmountOfGoldMedals = x.Events
						.SelectMany(ev => ev.Results.Select(x => x.Player.BirthDate.Year))
						.GroupBy(yr => yr)
						.Select(g => context.Results
							.Where(r => r.Event.Olympiс.Year == g.First() && r.Medal == "Gold")
							.Count())
				});

			return query.ToList().First();
		}

		/// <summary>
		/// Task 2
		/// Перечислите все индивидуальные (не групповые) соревнования, в которых была ничья в счете, 
		/// и два или более игрока выиграли золотую медаль
		/// </summary>
		/// <returns></returns>
		public IEnumerable<object> GetIndividualEventsWithDrawAndMoreThanTwoGolds()
		{

			var query = context.Results
				.GroupBy(r => r.EventId)
				.Where(grp => grp.Where(r => r.Medal == "Gold").Count() > 1)
				.Select(grp => grp.Where(r => r.Medal == "Gold")
					.GroupBy(y => y.Rezult)
					.Where(g => g.Count() > 1)
					.Select(x => new
					{
						EventId = x.First().EventId,
						EventName = x.First().Event.Name,
						DrawGoldenResult = x.Key,
						Results = x.ToArray()
					}));

			return query.ToList();
		}

		/// <summary>
		/// Task 3
		/// Найдите всех игроков, которые выиграли хотя бы одну медаль (GOLD, SILVER и BRONZE)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<object> GetPlayersWithUsefulMedals()
		{
			var query = context.Results.Where(x => x.Medal != "Chockolate").GroupBy(x => x.Player).Select(x => new
			{
				Player = x.Key,
				OlympId = x.First().Event.OlympicId,
				Medal = x.First().Medal,
			}).OrderBy(x => x.Player.Name);

			return query.ToList();
		}

		/// <summary>
		/// Task 4
		/// В какой стране был наибольший процент игроков, чьи имена начинались с гласной?
		/// </summary>
		/// <returns></returns>
		public object GetCountryWithMostVowelNamePlayers()
		{
			var vowels = new string[] { "A", "E", "I", "O", "U", "Y" };
			var query = context.Players
				.GroupBy(x => x.CountryId)
				.Select(cg => new
				{
					Country = cg.Key,
					Percentage = cg.Where(p => vowels.Contains(p.Name.Substring(0, 1))).Count() * 100.0 / cg.Count()
				}).OrderByDescending(y => y.Percentage);

			return query.Take(1);
		}

		/// <summary>
		/// Task 5
		/// Для Олимпийских игр данного года найдите 10 стран с минимальным соотношением количества групповых медалей к численности населения.
		/// </summary>
		/// <param name="year">Year for olympic games</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">Олимпиады с заданным годом проведения не найдены</exception>
		public IEnumerable<object> GetCountriesWithLessMedalPopulationPercentageAtYear(int year)
		{
			if (!context.Olympics.Any(x => x.Year == year))
				throw new ArgumentException("No Olympic games at such year");
			var ol = context.Olympics.Where(x => x.Year == year).FirstOrDefault();

			var query = context.Results.Where(r => r.Event.OlympicId == ol.OlympicId)
				.GroupBy(x => x.Player.Country)
				.Select(x => new
				{
					CountryName = x.Key.Name,
					Percentage = x.Where(r => r.Medal != "Chockolate" && r.Event.IsTeamEvent == 1).Count() * 100.0 / x.Key.Population
				})
				.OrderBy(y => y.Percentage)
				.Take(10);

			return query.ToList();
		}

		#endregion


		#region Working with DB

		/// <summary>
		/// Generating data with several amount of values
		/// </summary>
		/// <param name="cCountries">Amount of countries to generate</param>
		/// <param name="cOlympics">Amount of Olympic games to generate</param>
		/// <param name="cPlayers">Amount of players to generate</param>
		/// <param name="cEvents">Amount of events to generate</param>
		/// <param name="cResults">Amount of results to generate</param>
		public void GenerateData(int cCountries, int cOlympics, int cPlayers, int cEvents, int cResults)
		{
			countries = new();
			olympics = new();
			players = new();
			events = new();
			results = new();

			var rnd = new Randomizer();
			var eventsArray = new string[15].ToList().Select(x => Lorem.Words(3).ToArray()).ToList();
			var eventNames = eventsArray.Select(x => string.Join(' ', x)).ToList();
			var eventTypes = eventsArray.Select(x => $"{x[0][0]}{x[1][0]}{x[2][0]}".ToUpper()).ToList();
			var medals = new List<string>() { "Gold", "Silver", "Bronze", "Chockolate" };


			for (int i = 0; i < cCountries; ++i)
			{
				var reg = new RegionInfo("US");
				while (true)
					try
					{
						reg = new RegionInfo(new Bogus.DataSets.Address().CountryCode());
						// generate only unique countries
						if (countries.Any(x => x.CountryId == reg.ThreeLetterISORegionName))
							continue;
						break;
					}
					catch (Exception) { }

				countries.Add(new Country
				{
					Name = reg.EnglishName,
					CountryId = reg.ThreeLetterISORegionName,
					AreaSqkm = rnd.Number(10000, 1000000),
					Population = rnd.Number(100000, 40000000)
				});
			}

			for (int i = 0; i < cOlympics; i++)
			{
				var year = rnd.Number(1970, 2022);
				var start = new Bogus.DataSets.Date().Between(new DateTime(year, 1, 1), new DateTime(year, 11, 30));
				var country = rnd.ListItem(countries);

				// generate only unique olympics. If there is such id, change country of olympic
				while (olympics.Any(x => x.OlympicId == $"{country.CountryId}{year}"))
					country = rnd.ListItem(countries);

				var olymp = new Olympiс
				{
					OlympicId = $"{country.CountryId}{year}",
					City = Address.City(),
					Year = year,
					StartDate = start,
					EndDate = new Bogus.DataSets.Date().Between(start, new DateTime(year, 12, 30)),
				};
				olymp.AddCountry(country);
				olympics.Add(olymp);
			}

			for (int i = 0; i < cPlayers; ++i)
			{
				var name = new Bogus.DataSets.Name();
				var firstName = name.FirstName();
				var lastName = name.LastName();

				// generate only unique players. If there is such id, change lastname of player
				while (players.Any(x => $"{x.Name}" == $"{firstName} {lastName}"))
					lastName = name.LastName();

				var fn = $"{firstName}   ".Substring(0, 4).Replace(' ', '_').ToUpper();
				var ln = $"{lastName}   ".Substring(0, 4).Replace(' ', '_').ToUpper();

				var player = new Player
				{
					Name = $"{firstName} {lastName}",
					PlayerId = $"{fn}{ln}{rnd.Number(10, 99)}",
					BirthDate = new Bogus.DataSets.Date().Past(50, new DateTime(2000, 1, 1))
				};
				player.AddCountry(rnd.ListItem(countries));
				players.Add(player);
			}

			for (int i = 0; i < cEvents; i++)
			{
				var eventt = new Event
				{
					EventId = $"E{i}",
					Name = rnd.ListItem(eventNames),
					EventType = rnd.ListItem(eventTypes),
					IsTeamEvent = rnd.Number(1),
					NumPlayersInTeam = rnd.Number(1, 15),
					ResultNotedIn = Lorem.GetFirstWord()
				};
				eventt.AddOlympic(rnd.ListItem(olympics));
				events.Add(eventt);
			}

			for (int i = 0; i < cResults; ++i)
			{
				var eventt = rnd.ListItem(events);
				var player = rnd.ListItem(players);

				// generate only unique results. If there is such result, change player and event
				while (results.Any(x => x.Player == player && x.Event == eventt))
				{
					eventt = rnd.ListItem(events);
					player = rnd.ListItem(players);
				}

				var rez = rnd.Float(1, 100);
				var medal = rnd.ListItem(medals);
				// generating results with equal scores and medals with 40% chance (for task 2)
				if (rnd.Number(100) < 40 && results.Any(r => r.EventId == eventt.EventId))
				{
					var prev = rnd.ListItem(results.Where(r => r.EventId == eventt.EventId).ToList());
					rez = prev.Rezult;
					prev.Medal = medal;
				}

				var result = new Result
				{
					Medal = medal,
					Rezult = rez
				};
				result.AddEvent(eventt);
				result.AddPlayer(player);
				results.Add(result);
			}

			context.Countries.AddRange(countries);
			context.Olympics.AddRange(olympics);
			context.Players.AddRange(players);
			context.Events.AddRange(events);
			context.Results.AddRange(results);

			context.SaveChanges();
		}

		/// <summary>
		/// Clearing data from DB
		/// </summary>
		public void ClearAllData()
		{
			context.Countries.RemoveRange(context.Countries);
			context.Olympics.RemoveRange(context.Olympics);
			context.Players.RemoveRange(context.Players);
			context.Events.RemoveRange(context.Events);
			context.Results.RemoveRange(context.Results);
			context.SaveChanges();
		}

		#endregion
	}
}
