using System;

namespace Cida.Server.Infrastructure.Database.Models.DatabaseModels;

public class FtpInformation
{
    public string FtpPath { get; set; } = string.Empty;

    public Guid ModuleId { get; set; }

    public ModuleInformation Module { get; set; } = null!;
}
