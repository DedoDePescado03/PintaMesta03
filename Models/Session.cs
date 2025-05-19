using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;


namespace PintaMesta.Models
{
    [Table("sessions")]
    public class Session : BaseModel
    {
        [PrimaryKey("id", false), JsonIgnore]
        public string Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("host_user_id")]
        public string HostUserId { get; set; }

        [Column("drawer_user_id")]
        public string DrawerUserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("has_game_started")]
        public bool HasGameStarted { get; set; }

        [Column("current_word")]
        public string CurrentWord { get; set; }

        [Column("round_number")]
        public int RoundNumber { get; set; }

        [Column("current_drawing_id")]
        public string CurrentDrawingId { get; set; }

        [Column("has_game_ended")]
        public bool HasGameEnded {  get; set; }
    }
}
