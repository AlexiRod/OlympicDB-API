using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OlympDB.Classes
{
    public class Event
    {
        [Key]
        public string EventId { get; set; }
        public string Name { get; set; }
        public string EventType { get; set; }
        public string OlympicId { get; set; }
        public int IsTeamEvent { get; set; }
        public int NumPlayersInTeam { get; set; }
        public string ResultNotedIn { get; set; }

		[JsonIgnore]
        [ForeignKey("OlympicId")]
		public Olympiс Olympiс { get; set; }

		[JsonIgnore]
		public List<Result> Results { get; set; } = new();

        public void AddOlympic(Olympiс olymp)
        {
            OlympicId = olymp.OlympicId;
            Olympiс = olymp;
            olymp.Events.Add(this);
        }
    }
}
