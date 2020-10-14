using Module.IrcAnime.Avalonia.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Models = Module.IrcAnime.Avalonia.Models;

namespace Module.IrcAnime.Avalonia.Models
{
    public class Resolution
    {
        public string ResolutionType { get; set; }

        public List<Bot> Bots { get; set; } = new List<Bot>();

        public static Resolution FromPackInformation(PackNameInformation information, PackMetadata metadata)
        {
            var bot = Models.Bot.FromPackInformation(information, metadata);
            var resolution = new Resolution()
            {
                ResolutionType = information.Resolution ?? "unknown",
            };
            resolution.Bots.Add(bot);
            return resolution;
        }

        internal void Update(PackMetadata metadata, PackNameInformation information)
        {
            var botToUpdate = this.Bots.FirstOrDefault(x => x.Name == metadata.Bot);
            if (botToUpdate is null)
            {
                this.Bots.Add(Bot.FromPackInformation(information, metadata));
            }
            else
            {
                botToUpdate.Update(metadata, information);
            }
        }
    }
}
