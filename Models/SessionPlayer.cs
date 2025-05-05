using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;


namespace PintaMesta.Models
{
    [Table("session_players")]
    class SessionPlayer : BaseModel
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("session_id")]
        public string SessionId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; }

        [Column("is_drawer")]
        public bool IsDrawer { get; set; }
    }
}
