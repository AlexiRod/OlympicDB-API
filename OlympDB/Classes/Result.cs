using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace OlympDB.Classes
{
    public class Result
    {
        public string EventId { get; set; }
        public string PlayerId { get; set; }
        public string Medal { get; set; }
        public float Rezult { get; set; }

		[JsonIgnore]
        [ForeignKey("EventId")]
        public Event Event { get; set; }

		[JsonIgnore]
        [ForeignKey("PlayerId")]
        public Player Player { get; set; }

        public void AddEvent(Event eventt)
        {
            EventId = eventt.EventId;
            Event = eventt;
            eventt.Results.Add(this);
        }

        public void AddPlayer(Player player)
        {
            PlayerId = player.PlayerId;
            Player = player;
            player.Results.Add(this);
        }
    }
}
