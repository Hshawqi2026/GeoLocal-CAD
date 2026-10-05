using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.Core;

namespace GeoLocalCAD.Plugin.Commands
{
    public sealed class GeoLocalCommand
    {
        [CommandMethod(PluginInfo.CommandName, CommandFlags.Modal)]
        public void Execute()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
                throw new InvalidOperationException("GEOLOCAL could not run because there is no active drawing document.");

            var editor = document.Editor;
            try
            {
                if (PluginEntryPoint.Logger == null)
                    throw new InvalidOperationException("The GeoLocal CAD plugin was not initialized by AutoCAD.");

                PluginEntryPoint.Logger.Information("GEOLOCAL command started.");
                editor.WriteMessage("\nGeoLocal CAD v{0} loaded successfully.", PluginInfo.ProductVersion);
                editor.WriteMessage("\nOffline GIS/Survey engine initialized. Phase 1 plugin loader is active.");
                editor.WriteMessage("\nLog: {0}\n", PluginEntryPoint.Logger.LogFilePath);
                PluginEntryPoint.Logger.Information("GEOLOCAL command completed successfully.");
            }
            catch (Exception ex)
            {
                PluginEntryPoint.Logger?.Error("GEOLOCAL command failed.", ex);
                editor.WriteMessage("\nGEOLOCAL failed. What: command execution error. Where: GEOLOCAL. " +
                    "Likely cause: plugin initialization or AutoCAD document state. " +
                    "Action: review the GeoLocal CAD log and report the exception.\n");
                throw;
            }
        }
    }
}
