using Module.IrcAnime.Avalonia.Services;
using System;
using Models = Module.IrcAnime.Avalonia.Models;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Module.IrcAnime.Avalonia.Models
{
    public class Pack
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public string EpisodeNumber { get; set; }

        public string Identifier => this.Name + this.EpisodeNumber;

        public List<Resolution> Resolutions { get; set; } = new List<Resolution>();

        public void Update(PackMetadata metadata, PackNameInformation information)
        {
            this.Size = metadata.Size;

            var resolutionToUpdate = this.Resolutions.FirstOrDefault(x => x.ResolutionType == information.Resolution);
            if (resolutionToUpdate is null)
            {
                this.Resolutions.Add(Resolution.FromPackInformation(information, metadata));
            }
            else
            {
                resolutionToUpdate.Update(metadata, information);
            }
        }

        public static Pack FromPackInformation(PackNameInformation information, PackMetadata metadata)
        {
            var resolution = Models.Resolution.FromPackInformation(information, metadata);
            var pack = new Pack()
            {
                Name = information.Name,
                Size = metadata.Size,
                EpisodeNumber = information.EpisodeNumber ?? "unknown",
            };
            pack.Resolutions.Add(resolution);
            return pack;
        }

    }
}
