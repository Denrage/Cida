using System;
using System.Collections.Generic;
using System.Text;

namespace Module.IrcAnime.Avalonia.Models
{
    public class Pack
    {
        public string Name { get; set; }

        public Dictionary<string, ulong> Packs { get; set; } = new Dictionary<string, ulong>();

        public ulong Size { get; set; }
    }
}
