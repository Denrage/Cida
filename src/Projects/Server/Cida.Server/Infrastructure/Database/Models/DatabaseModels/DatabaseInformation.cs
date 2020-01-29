using System;

namespace Cida.Server.Infrastructure.Database.Models.DatabaseModels
{
    public class DatabaseInformation
    {
        public string DatabaseName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }



        public Guid ModuleId { get; set; }

        public ModuleInformation Module { get; set; }
    }
}
