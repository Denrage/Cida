using Module.IrcAnime.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.IrcAnime.Avalonia.Services
{
    public interface IPackNameInformationExtractor
    {
        PackNameInformation GetInformation(string filename);
    }
}
