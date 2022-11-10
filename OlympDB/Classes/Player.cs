using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OlympDB.Classes
{
    public class Player
    {
        public string Name { get; set; }
        [Key]
        public string PlayerId { get; set; }
        public string CountryId { get; set; }
        public DateTime BirthDate { get; set; }

		[JsonIgnore]
        [ForeignKey("CountryId")]
        public Country Country { get; set; }

		[JsonIgnore]
		public List<Result> Results { get; set; } = new();

        public void AddCountry(Country country)
        {
            CountryId = country.CountryId;
            Country = country;
            country.Players.Add(this);
        }
    }
}
