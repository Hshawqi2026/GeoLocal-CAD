using System;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.Core.Logging;

[assembly: ExtensionPlugin(typeof(GeoLocalCAD.Plugin.PluginEntryPoint))]

namespace GeoLocalCAD.Plugin
{
    public sealed class PluginEntryPoint : IExtensionApplication
    {
        internal static FileGeoLogger Logger { get; private set; }

        public void Initialize()
        {
            Logger = new FileGeoLogger();
            Logger.Information("GeoLocal CAD plugin loading. Version 0.1.0.");
        }

        public void Terminate()
        {
            if (Logger != null)
            {
                Logger.Information("GeoLocal CAD plugin terminating.");
                Logger.Dispose();
            }
        }
    }
}
