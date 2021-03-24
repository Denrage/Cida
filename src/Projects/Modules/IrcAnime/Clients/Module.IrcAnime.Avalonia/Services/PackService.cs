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
        private readonly Dictionary<string, Pack> packs = new Dictionary<string, Pack>();

        public Pack Get(PackMetadata packMetadata)
        {
            if (!this.packs.TryGetValue(packMetadata.Name, out var result))
            {
                var pack = new Pack()
                {
                    Name = packMetadata.Name,
                    Size = packMetadata.Size,
                };

                this.packs[packMetadata.Name] = pack;
                result = pack;
            }

            if (packMetadata.Bot != null)
            {
                result.Packs[packMetadata.Bot] = packMetadata.Number;
            }

            return result;
        }

        public void Update(PackMetadata packMetadata)
        {
            if (this.packs.TryGetValue(packMetadata.Name, out var result))
            {
                if (packMetadata.Bot != null)
                {
                    result.Packs[packMetadata.Bot] = packMetadata.Number;
                }
            }
        }
    }
}
