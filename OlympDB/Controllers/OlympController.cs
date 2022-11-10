using Microsoft.AspNetCore.Mvc;
using OlympDB.Database;

namespace OlympDB.Controllers
{
    [ApiController]
	[Route("api/olymp")]
	public class OlympController : Controller
    {
		private readonly OlympDbRepository repository;

		public OlympController(OlympDbContext dbContext)
        {
            repository = new OlympDbRepository(dbContext);
        }

		#region Database working

		/// <summary>
		/// Генерация данных в БД с заданным количеством записей
		/// </summary>
		/// <param name="countries">Количество генерируемых стран</param>
		/// <param name="olymps">Количество генерируемых олимпиад</param>
		/// <param name="players">Количество генерируемых игроков</param>
		/// <param name="events">Количество генерируемых соревнований</param>
		/// <param name="results">Количество генерируемых результатов</param>
		/// <response code="200">Данные сгенерированы, возвращается общая информация о них</response>
		[HttpPost("generate/{countries}/{olymps}/{players}/{events}/{results}")]
		[ProducesResponseType(200)]
		public IActionResult Regenerate(int countries, int olymps, int players, int events, int results)
		{
			repository.ClearAllData();
			repository.GenerateData(countries, olymps, players, events, results);
			return PrintInfo();
		}


		/// <summary>
		/// Информация о количестве записей в каждой из таблиц
		/// </summary>
		/// <response code="200">Информация о сгенерированных данных</response>
		[HttpGet("info/")]
		[ProducesResponseType(200)]
		public IActionResult GetInfo()
        {
            if (repository.Countries.FirstOrDefault() == null)
            {
                return Ok("No data in database. Call /generate url to generate it");
            }
            return PrintInfo();
		}

		/// <summary>
		/// Очистка всех данных 
		/// </summary>
		/// <response code="200">Все таблицы очищены</response>
		[HttpGet("clear/")]
		[ProducesResponseType(200)]
		public IActionResult ClearData()
        {
			repository.ClearAllData();
            return Ok("Data has been cleared.");
		}

		/// <summary>
		/// Данные из таблицы стран
		/// </summary>
		/// <response code="200">Список стран в формате json</response>
		[HttpGet("countries/")]
		[ProducesResponseType(200)]
		public IActionResult GetCountries()
        {
            return Ok(repository.Countries);
        }

		/// <summary>
		/// Данные из таблицы олимпиад
		/// </summary>
		/// <response code="200">Список олимпиад в формате json</response>
		[HttpGet("olymps/")]
		[ProducesResponseType(200)]
		public IActionResult GetOlympics()
        {
            return Ok(repository.Olympics);
        }

		/// <summary>
		/// Данные из таблицы игроков
		/// </summary>
		/// <response code="200">Список игроков в формате json</response>
		[HttpGet("players/")]
		[ProducesResponseType(200)]
		public IActionResult GetPlayers()
        {
            return Ok(repository.Players);
        }

		/// <summary>
		/// Данные из таблицы соревнований
		/// </summary>
		/// <response code="200">Список соревнований в формате json</response>
		[HttpGet("events/")]
		[ProducesResponseType(200)]
		public IActionResult GetEvents()
        {
            return Ok(repository.Events);
        }

		/// <summary>
		/// Данные из таблицы результатов
		/// </summary>
		/// <response code="200">Список результатов в формате json</response>
		[HttpGet("results/")]
		[ProducesResponseType(200)]
		public IActionResult GetResults()
        {
            return Ok(repository.Results);
        }


		/// <summary>
		/// Приватный метод для вывода информации о таблицах
		/// </summary>
		private IActionResult PrintInfo()
		{
			var years = string.Join(", ", repository.Olympics.Select(x => x.Year));
			return Ok("Amount of data from database:\n" +
					$"Countries: {repository.Countries.Count()}\n" +
					$"Olympics: {repository.Olympics.Count()}\n" +
					$"Players: {repository.Players.Count()}\n" +
					$"Events: {repository.Events.Count()}\n" +
					$"Results: {repository.Results.Count()}\n\n" +
					$"Notice, that Olympic games years for tasks could be as follows:\n{years}");
		}

		#endregion


		#region Tasks

		/// <summary>
		/// Task 1
		/// Для Олимпийских игр заданного года сгенерируйте список (год рождения, количество игроков, количество золотых медалей), 
		/// содержащий годы, в которые родились игроки, количество игроков, родившихся в каждый из этих лет, которые выиграли 
		/// по крайней мере одну золотую медаль, и количество золотых медалей, завоеванных игроками, родившимися в этом году
		/// </summary>
		/// <param name="year">Год олимпиады, по-умолчанию год первой олимпиады из БД</param>
		/// <response code="200">Результат задания 1</response>
		/// <response code="404">Олимпиады с заданным годом проведения не найдены</response>
		[HttpGet("task/playerslist/")]
		[ProducesResponseType(200)]
		[ProducesResponseType(404)]
		public IActionResult GetPlayersList(int? year)
        {
			year ??= repository.Olympics.First().Year;
			try
			{
				return Ok(repository.GetOlympicPlayersListAtYear(year.Value));
			}
			catch (Exception ex)
			{
				return NotFound(ex.Message);
			}
        }

		/// <summary>
		/// Task 2
		/// Перечислите все индивидуальные (не групповые) соревнования, в которых была ничья в счете, 
		/// и два или более игрока выиграли золотую медаль
		/// </summary>
		/// <response code="200">Результат задания 2</response>
		[HttpGet("task/eventsdraw/")]
		[ProducesResponseType(200)]
		public IActionResult GetEventsDraw()
        {
			return Ok(repository.GetIndividualEventsWithDrawAndMoreThanTwoGolds());
        }

		/// <summary>
		/// Task 3
		/// Найдите всех игроков, которые выиграли хотя бы одну медаль (GOLD, SILVER и BRONZE)
		/// </summary>
		/// <response code="200">Результат задания 3</response>
		[HttpGet("task/playersmedals/")]
		[ProducesResponseType(200)]
		public IActionResult GetPlayersWithMedals()
        {
			return Ok(repository.GetPlayersWithUsefulMedals());
        }

		/// <summary>
		/// Task 4
		/// В какой стране был наибольший процент игроков, чьи имена начинались с гласной?
		/// </summary>
		/// <response code="200">Результат задания 4</response>
		[HttpGet("task/countryvowel/")]
		[ProducesResponseType(200)]
		public IActionResult GetCountryWithVowelPlayers()
        {
			return Ok(repository.GetCountryWithMostVowelNamePlayers());
        }

		/// <summary>
		/// Task 5
		/// Для Олимпийских игр заданного года найдите 5 стран с минимальным соотношением количества групповых медалей к численности населения.
		/// </summary>
		/// <param name="year">Год олимпиады, по-умолчанию год первой олимпиады из БД</param>
		/// <response code="200">Результат задания 5</response>
		/// <response code="404">Олимпиады с заданным годом проведения не найдены</response>
		[HttpGet("task/countrieslessmedals/")]
		[ProducesResponseType(200)]
		[ProducesResponseType(404)]
		public IActionResult GetCountriesWithLessMedalPopulationPercentage(int? year)
		{
			year ??= repository.Olympics.First().Year;
			try
			{
				return Ok(repository.GetCountriesWithLessMedalPopulationPercentageAtYear(year.Value));
			}
			catch (Exception ex)
			{
				return NotFound(ex.Message);
			}
        }

		#endregion
	}
}
