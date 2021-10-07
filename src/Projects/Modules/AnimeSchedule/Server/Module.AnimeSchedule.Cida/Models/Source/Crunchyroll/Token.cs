using System;

namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class Token
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromSeconds(1);

        public static implicit operator string(Token token)
        {
            return token.AccessToken;
        }
    }
}
