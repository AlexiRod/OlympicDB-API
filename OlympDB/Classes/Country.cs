using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OlympDB.Classes
{
    public class Country
    {
        public string Name { get; set; }
        [Key]
        public string CountryId { get; set; }
        public int AreaSqkm { get; set; }
        public int Population { get; set; }

		[JsonIgnore]
		public List<Olympiс> Olympiсs { get; set; } = new();
		[JsonIgnore]
		public List<Player> Players { get; set; } = new();
    }
}
