using Module.IrcAnime.Avalonia.Services;
using System;

namespace Module.IrcAnime.Avalonia.Models
{
    public class UploaderGroup
    {
        public string Name { get; set; }

        public string Filename { get; set; }

        public ulong PackageNumber { get; set; }

        public static UploaderGroup FromPackInformation(PackNameInformation information, PackMetadata metadata)
        {
            return new UploaderGroup()
            {
                Name = information.Group ?? "unknown",
                PackageNumber = metadata.Number,
                Filename = metadata.Name,
            };
        }

        internal void Update(PackMetadata metadata)
        {
            this.PackageNumber = metadata.Number;
        }
    }
}
