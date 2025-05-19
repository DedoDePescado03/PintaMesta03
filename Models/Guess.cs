using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PintaMesta.Models
{
    [Table("guesses")]
    public class Guess : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("session_id")]
        public string SessionId { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("guess")]
        public string GuessText { get; set; }
    }
}
