using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.Core.Settings;
using GeoLocalCAD.UI;

namespace GeoLocalCAD.Plugin.Commands
{
    public sealed class GeoSettingsCommand
    {
        [CommandMethod("GEOSETTINGS", CommandFlags.Modal)]
        public void Execute()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
                throw new InvalidOperationException("GEOSETTINGS could not run because there is no active drawing document.");

            try
            {
                PluginEntryPoint.Logger?.Information("GEOSETTINGS command started.");
                var window = new GeoSettingsWindow(new GeoSettingsStore(), PluginEntryPoint.Logger);
                Application.ShowModalWindow(window);
                PluginEntryPoint.Logger?.Information("GEOSETTINGS command completed.");
            }
            catch (Exception exception)
            {
                PluginEntryPoint.Logger?.Error("GEOSETTINGS command failed.", exception);
                document.Editor.WriteMessage("\nGEOSETTINGS failed. What: settings window could not be opened. " +
                    "Where: GEOSETTINGS. Likely cause: WPF initialization or local settings access. " +
                    "Action: review the GeoLocal CAD log.\n");
                throw;
            }
        }
    }
}
