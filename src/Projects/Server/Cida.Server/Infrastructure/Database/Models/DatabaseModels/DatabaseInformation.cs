using System;

namespace Cida.Server.Infrastructure.Database.Models.DatabaseModels;

public class DatabaseInformation
{
    public string DatabaseName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public Guid ModuleId { get; set; }

    public ModuleInformation Module { get; set; } = null!;
}
