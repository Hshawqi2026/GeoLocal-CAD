using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using GeoLocalCAD.Core.Logging;
using GeoLocalCAD.Plugin.Ribbon;

[assembly: ExtensionPlugin(typeof(GeoLocalCAD.Plugin.PluginEntryPoint))]

namespace GeoLocalCAD.Plugin
{
    public sealed class PluginEntryPoint : IExtensionApplication
    {
        internal static FileGeoLogger Logger { get; private set; }

        public void Initialize()
        {
            Logger = new FileGeoLogger();
            Logger.Information("GeoLocal CAD plugin loading. Version " + GeoLocalCAD.Core.PluginInfo.ProductVersion + ".");
            if (ComponentManager.Ribbon == null)
                ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
            else
                GeoLocalRibbon.EnsureCreated();
        }

        private static void ComponentManager_ItemInitialized(object sender, RibbonItemEventArgs e)
        {
            if (ComponentManager.Ribbon == null) return;
            ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
            GeoLocalRibbon.EnsureCreated();
        }

        public void Terminate()
        {
            ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
            if (Logger != null)
            {
                Logger.Information("GeoLocal CAD plugin terminating.");
                Logger.Dispose();
            }
        }
    }
}
