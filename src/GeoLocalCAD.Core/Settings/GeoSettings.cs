using System;
using System.IO;
using System.Xml.Serialization;

namespace GeoLocalCAD.Core.Settings
{
    [Serializable]
    public sealed class GeoSettings
    {
        public bool OfflineMode { get; set; }
        public string DrawingCrs { get; set; }
        public string DataDirectory { get; set; }

        public GeoSettings()
        {
            OfflineMode = true;
            DrawingCrs = string.Empty;
            DataDirectory = string.Empty;
        }
    }

    public sealed class GeoSettingsStore
    {
        private readonly string _settingsPath;

        public GeoSettingsStore(string applicationName = "GeoLocalCAD")
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                applicationName);
            Directory.CreateDirectory(directory);
            _settingsPath = Path.Combine(directory, "settings.xml");
        }

        public string SettingsPath { get { return _settingsPath; } }

        public GeoSettings Load()
        {
            if (!File.Exists(_settingsPath))
                return new GeoSettings();

            try
            {
                using (var stream = File.OpenRead(_settingsPath))
                {
                    var serializer = new XmlSerializer(typeof(GeoSettings));
                    return (GeoSettings)serializer.Deserialize(stream);
                }
            }
            catch (Exception exception)
            {
                throw new InvalidDataException(
                    "GeoLocal CAD settings could not be read. The settings file may be corrupt; use the Settings window to recreate it.",
                    exception);
            }
        }

        public void Save(GeoSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            var temporaryPath = _settingsPath + ".tmp";
            try
            {
                using (var stream = File.Create(temporaryPath))
                {
                    var serializer = new XmlSerializer(typeof(GeoSettings));
                    serializer.Serialize(stream, settings);
                }
                File.Copy(temporaryPath, _settingsPath, true);
                File.Delete(temporaryPath);
            }
            catch (Exception exception)
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
                throw new IOException(
                    "GeoLocal CAD settings could not be saved. Verify that the local application data folder is writable.",
                    exception);
            }
        }
    }
}
