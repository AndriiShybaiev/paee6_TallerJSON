using Newtonsoft.Json;

namespace DataAccess
{
    public class Provincia
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("nm")]
        public string Nombre { get; set; } = string.Empty;
    }
}
