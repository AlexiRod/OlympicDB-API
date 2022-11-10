using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OlympDB.Classes
{
    public class Olympiс
    {
        [Key]
        public string OlympicId { get; set; }
        public string CountryId { get; set; }
        public string City { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

		[JsonIgnore]
        [ForeignKey("CountryId")]
        public Country Country { get; set; }

		[JsonIgnore]
		public List<Event> Events { get; set; } = new();

        public void AddCountry(Country country)
        {
            CountryId = country.CountryId;
            Country = country;
            country.Olympiсs.Add(this);
        }
    }
}
