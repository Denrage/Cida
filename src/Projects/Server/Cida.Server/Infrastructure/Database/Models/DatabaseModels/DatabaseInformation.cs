using System;

namespace Cida.Server.Infrastructure.Database.Models.DatabaseModels;

public record DatabaseInformation(string DatabaseName, string Username, string Password, Guid ModuleId, ModuleInformation Module);

//public class DatabaseInformation
//{
//    public string DatabaseName { get; set; }

//    public string Username { get; set; }

//    public string Password { get; set; }



//    public Guid ModuleId { get; set; }

//    public ModuleInformation Module { get; set; }
//}
