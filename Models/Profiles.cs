using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PintaMesta.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

    }
}
