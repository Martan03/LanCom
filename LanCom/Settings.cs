using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanCom
{
    internal class Settings
    {
        public string? defaultDir { get; set; }
        public string? defaultIP { get; set; }
        public Dictionary<string, string>? IPShortcuts { get; set; }

        public Settings()
        {
            IPShortcuts = new Dictionary<string, string>();
        }

        public void SaveSettings()
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText("settings.json", json);
        }

        public void LoadSettings()
        {
            if (!File.Exists("settings.json"))
                return;
            string json = File.ReadAllText("settings.json");
            
            Settings? data = JsonSerializer.Deserialize<Settings>(json);
            if (data == null)
                return;

            defaultDir = data.defaultDir;
            defaultIP = data.defaultIP;
            IPShortcuts = data.IPShortcuts;
        }
    }
}
