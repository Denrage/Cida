using System.IO;
using Cida.Server.Interfaces;

namespace Cida.Server.Console
{
    public class FileWriter : ISettingsWriter
    {
        private readonly string settingsFile;

        public FileWriter(string settingsFile)
        {
            this.settingsFile = settingsFile;

            if (!Directory.Exists(Path.GetDirectoryName(this.settingsFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFile));
            }
            
            if (!File.Exists(settingsFile))
            {
                File.WriteAllText(settingsFile, string.Empty);
            }
        }

        public void Save(string settings) 
            => File.WriteAllText(this.settingsFile, settings);

        public string Get() 
            => File.ReadAllText(this.settingsFile);
    }
}