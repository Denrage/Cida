using Module.IrcAnime.Avalonia.Models;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Module.IrcAnime.Avalonia.Services
{
    public class PackNameInformationExtractor : IPackNameInformationExtractor
    {
        private readonly RegularExpressions expressions;
        public string Identifier { get; }

        public PackNameInformationExtractor(string identifier, RegularExpressions expressions)
        {
            this.Identifier = identifier;
            this.expressions = expressions;
        }

        public PackNameInformation GetInformation(string filename)
        {
            var result = new PackNameInformation
            {
                Name = filename,
                OriginalName = filename,
            };

            var match = new Regex(this.expressions.Group).Match(result.Name);
            if (match.Success)
            {
                result.Group = match.Value.Replace("[", "").Replace("]", "");
                result.Name = result.Name.Replace(match.Value, "");
            }

            match = new Regex(this.expressions.Resolution).Match(result.Name);
            if (match.Success)
            {
                result.Resolution = match.Value.Replace("[", "").Replace("]", "");
                result.Name = result.Name.Replace(match.Value, "");
            }

            match = new Regex(this.expressions.FileExtension).Match(result.Name);
            if (match.Success)
            {
                result.FileExtension = match.Value.Replace("[", "").Replace("]", "").Replace(".", "");
                result.Name = result.Name.Replace(match.Value, "");
            }

            match = new Regex(this.expressions.EpisodeNumber).Match(result.Name);
            if (match.Success)
            {
                result.EpisodeNumber = match.Value.Replace("[", "").Replace("]", "");
                result.Name = result.Name.Replace(match.Value, "");
            }

            foreach (var regex in this.expressions.Remove)
            {
                match = new Regex(regex).Match(result.Name);
                if (!string.IsNullOrEmpty(match.Value))
                {
                    result.Name = result.Name.Replace(match.Value, "");
                }
            }

            result.Name = result.Name.Trim();

            return result;
        }
    }
}
