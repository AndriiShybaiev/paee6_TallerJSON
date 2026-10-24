using Newtonsoft.Json;

namespace DataAccess
{
    public class Provincia
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("nm")]
        public string Nombre { get; set; } = string.Empty;

        [JsonProperty("Poblacion")]
        public long? Poblacion { get; set; }

        [JsonProperty("SuperficieKm2")]
        public decimal? SuperficieKm2 { get; set; }

        [JsonProperty("EscudoPath")]
        public string? EscudoPath { get; set; }

        [JsonProperty("MapaPath")]
        public string? MapaPath { get; set; }
    }
}
