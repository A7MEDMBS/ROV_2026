using System.IO;
using System.Text.Json;

namespace ROV_GUI_Control.Configuration
{
    public static class ConfigManager
    {
        private static string filePath = "config.json";
        public static AppConfig Current { get; private set; }
        public static void Load()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Current = JsonSerializer.Deserialize<AppConfig>(json);
            }
            else
            {
                Current = new AppConfig();
                Save();
            }
        }
        public static void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(Current, options);
            File.WriteAllText(filePath, json);
        }
    }
}
