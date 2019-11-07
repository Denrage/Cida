using System;
using System.Collections.Generic;
using System.Text;

namespace Module.DatabaseConnector.Models.DatabaseModels
{
    public class ModuleInformation
    {
        public string ModuleId { get; set; }

        public string ModuleName { get; set; }

        public string DatabaseName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
