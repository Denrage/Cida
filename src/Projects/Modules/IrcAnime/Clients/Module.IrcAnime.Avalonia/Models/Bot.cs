using Module.IrcAnime.Avalonia.Services;
using Models = Module.IrcAnime.Avalonia.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Module.IrcAnime.Avalonia.Models
{
    public class Bot
    {
        public string Name { get; set; }

        public List<UploaderGroup> UploaderGroup { get; set; } = new List<UploaderGroup>();

        public static Bot FromPackInformation(PackNameInformation information, PackMetadata metadata)
        {
            var group = Models.UploaderGroup.FromPackInformation(information, metadata);
            var bot = new Bot()
            {
                Name = metadata.Bot ?? "unknown",
            };
            bot.UploaderGroup.Add(group);
            return bot;
        }

        internal void Update(PackMetadata metadata, PackNameInformation information)
        {
            var groupToUpdate = this.UploaderGroup.FirstOrDefault(x => x.Name == metadata.Bot);
            if (groupToUpdate is null)
            {
                this.UploaderGroup.Add(Models.UploaderGroup.FromPackInformation(information, metadata));
            }
            else
            {
                groupToUpdate.Update(metadata);
            }
        }
    }
}
