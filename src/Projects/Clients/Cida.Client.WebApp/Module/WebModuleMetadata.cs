using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cida.Client.WebApp
{
    public class WebModuleMetadata
    {
        [JsonPropertyName("startupHtmlFileName")]
        public string StartupHtmlFileName { get; set; }
    }
}
