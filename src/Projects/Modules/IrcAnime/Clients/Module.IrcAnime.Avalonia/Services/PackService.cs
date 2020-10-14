using Cida.Client.Avalonia.Api;
using Module.IrcAnime.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.Services
{
    //TODO: Interface this
    public class PackService
    {
        private readonly IModuleSettingsService settingsService;
        private readonly Dictionary<string, Pack> packs = new Dictionary<string, Pack>();

        public PackService(IModuleSettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        private async Task<PackNameInformation> ExtractInformation(PackMetadata metadata)
        {
            var extractorConfiguration = (await this.settingsService.Get<PackNameInformationExtractors>()).Configurations.FirstOrDefault(x => metadata.Name.Contains(x.Identifier));

            PackNameInformation information;
            if (extractorConfiguration is null)
            {
                // TODO: Log-Warning
                information = new PackNameInformation()
                {
                    Name = metadata.Name,
                };
            }
            else
            {
                var extractor = new PackNameInformationExtractor(extractorConfiguration.Identifier, extractorConfiguration.Expressions);
                information = extractor.GetInformation(metadata.Name);
            }

            return information;
        }

        public async Task<Pack> GetAsync(PackMetadata metadata)
        {
            var information = await this.ExtractInformation(metadata);

            if (this.packs.TryGetValue(information.Name+information.EpisodeNumber, out var pack))
            {
                var resolution = pack.Resolutions.FirstOrDefault(x => x.ResolutionType == information.Resolution);
                if (resolution is null)
                {
                    pack.Resolutions.Add(Resolution.FromPackInformation(information, metadata));
                }
                else
                {
                    var packBot = resolution.Bots.FirstOrDefault(x => x.Name == metadata.Bot);
                    if (packBot is null)
                    {
                        resolution.Bots.Add(Bot.FromPackInformation(information, metadata));
                    }
                    else
                    {
                        var group = packBot.UploaderGroup.FirstOrDefault(x => x.Name == information.Group);
                        if (group is null)
                        {
                            packBot.UploaderGroup.Add(UploaderGroup.FromPackInformation(information, metadata));
                        }
                    }
                }
            }
            else
            {
                pack = Pack.FromPackInformation(information, metadata);
                this.packs.Add(pack.Identifier, pack);
            }

            return pack;
        }

        public async Task UpdatePack(PackMetadata metadata)
        {
            var information = await this.ExtractInformation(metadata);

            if (!this.packs.TryGetValue(information.Name+information.EpisodeNumber, out var pack))
            {
                throw new InvalidOperationException("Pack needs to be in cache in order to update it!");
            }

            pack.Update(metadata, information);
        }
    }

    public class PackNameInformationExtractors
    {
        public List<PackNameInformationExtractorConfiguration> Configurations { get; set; } = new List<PackNameInformationExtractorConfiguration>()
        {
            new PackNameInformationExtractorConfiguration()
            {
                Identifier = "DEFAULT",
                Expressions = new RegularExpressions()
                {
                    EpisodeNumber = "DEFAULT EPISODE",
                    FileExtension = "DEFAULT FileExtension",
                    Group = "DEFAULT Group",
                    Resolution = "DEFAULT Resolution",
                }
            }
        };
    }

    public class PackNameInformationExtractorConfiguration
    {
        public string Identifier { get; set; }

        public RegularExpressions Expressions { get; set; }
    }
}
