using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.Models.DatabaseModels
{
    public class ModuleInformation
    {
        public Guid ModuleId { get; set; }

        public string ModuleName { get; set; }

        public FtpInformation FtpInfomation { get; set; }

        public DatabaseInformation DatabaseInformation { get; set; }
    }
}
