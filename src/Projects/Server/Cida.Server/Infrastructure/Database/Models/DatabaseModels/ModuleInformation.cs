using System;

namespace Cida.Server.Infrastructure.Database.Models.DatabaseModels;

public class ModuleInformation
{
    public Guid ModuleId { get; set; }

    public string ModuleName { get; set; } = string.Empty;

    public FtpInformation FtpInfomation { get; set; } = null!;

    public DatabaseInformation DatabaseInformation { get; set; } = null!;
}
