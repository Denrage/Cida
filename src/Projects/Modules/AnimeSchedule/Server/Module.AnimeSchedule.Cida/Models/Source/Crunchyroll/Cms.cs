namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class Cms
    {
        [System.Text.Json.Serialization.JsonPropertyName("bucket")]
        public string Bucket { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("policy")]
        public string Policy { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("signature")]
        public string Signature { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("key_pair_id")]
        public string KeyPairId { get; set; }
    }
}
