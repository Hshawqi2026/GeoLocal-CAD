using System;
using GeoLocalCAD.Core.Settings;

namespace GeoLocalCAD.Core.Tests
{
    internal static class GeoSettingsStoreSmokeTest
    {
        private static int Main()
        {
            var store = new GeoSettingsStore("GeoLocalCAD-Test");
            var settings = new GeoSettings
            {
                OfflineMode = false,
                DrawingCrs = "EPSG:32638",
                DataDirectory = "local-data"
            };
            store.Save(settings);
            var loaded = store.Load();
            Assert(loaded.OfflineMode == false, "OfflineMode did not persist.");
            Assert(loaded.DrawingCrs == "EPSG:32638", "DrawingCrs did not persist.");
            Assert(loaded.DataDirectory == "local-data", "DataDirectory did not persist.");
            Console.WriteLine("GeoSettingsStore smoke test passed: " + store.SettingsPath);
            GeoPhase3SmokeTest.Run();
            GeoRasterSmokeTest.Run();
            return 0;
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
