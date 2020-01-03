using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.Models.DatabaseModels
{
    public class FtpInformation
    {
        public string FtpPath { get; set; }



        public Guid ModuleId { get; set; }

        public ModuleInformation Module { get; set; }
    }
}
