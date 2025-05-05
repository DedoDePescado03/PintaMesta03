using System.Text.Json.Serialization;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;


namespace PintaMesta.Models
{
    [Table("sessions")]
    public class Session : BaseModel
    {
        [Column("id"), JsonIgnore]
        public string Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("host_user_id")]
        public string HostUserId { get; set; }

        [Column("drawer_user_id")]
        public string DrawerUserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("current_word")]
        public string CurrentWord { get; set; }
    }
}
