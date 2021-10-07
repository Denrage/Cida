namespace Cida.Server.Infrastructure.Database.Models;

public class FtpInformation
{
    public string FtpPath { get; set; } = string.Empty;

    public Guid ModuleId { get; set; }

    public ModuleInformation Module { get; set; } = null!;
}
