using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Supabase;
using PintaMesta.Models;


namespace PintaMesta.Models
{
    [Table("drawing_data")]
    public class DrawingData : BaseModel
    {
        [PrimaryKey("id"), Column("id")]
        public string Id { get; set; }

        [Column("session_id")]
        public string SessionId { get; set; }

        [Column("points")]
        public string Points { get; set; }

        [Column("line_color")]
        public string LineColor { get; set; }

        [Column("line_width")]
        public float LineWidth { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_cleared")]
        public bool IsCleared { get; set; }

        [JsonIgnore]
        internal List<PointData> PointList
        {
            get => string.IsNullOrEmpty(Points)
                ? new List<PointData>()
                : JsonSerializer.Deserialize<List<PointData>>(Points);
            set => Points = JsonSerializer.Serialize(value);
        }
    }

    public class PointData
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class SerializedLine
    {
        public List<PointData> Points { get; set; }
        public string Color { get; set; }
        public float Width { get; set; }
    }

}
