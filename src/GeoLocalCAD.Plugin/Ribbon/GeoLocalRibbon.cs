using System;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Windows;

namespace GeoLocalCAD.Plugin.Ribbon
{
    internal static class GeoLocalRibbon
    {
        private const string TabId = "GeoLocalCAD.RibbonTab";
        private static bool _isCreated;

        public static void EnsureCreated()
        {
            if (_isCreated || ComponentManager.Ribbon == null)
                return;

            var ribbon = ComponentManager.Ribbon;
            if (ribbon.FindTab(TabId) != null)
            {
                _isCreated = true;
                return;
            }

            var tab = new RibbonTab { Id = TabId, Title = "GeoLocal" };
            AddPanel(tab, "Maps", new RibbonButton { Text = "Local Maps", ShowText = true, CommandHandler = new AutoCadCommand("GEOLOCAL") });
            AddPanel(tab, "CRS", new RibbonButton { Text = "Settings", ShowText = true, CommandHandler = new AutoCadCommand("GEOSETTINGS") });
            AddPanel(tab, "Import/Export", new RibbonButton { Text = "Import GeoJSON", ShowText = true, CommandHandler = new AutoCadCommand("GEOIMPORT") });
            AddPanel(tab, "Import/Export", new RibbonButton { Text = "Export GeoJSON", ShowText = true, CommandHandler = new AutoCadCommand("GEOEXPORT") });
            AddPanel(tab, "Georeference", new RibbonButton { Text = "Georeference", ShowText = true, CommandHandler = new AutoCadCommand("GEOLOCAL") });
            AddPanel(tab, "DEM", new RibbonButton { Text = "DEM Tools", ShowText = true, CommandHandler = new AutoCadCommand("GEOLOCAL") });
            AddPanel(tab, "Survey", new RibbonButton { Text = "Coordinate Inspector", ShowText = true, CommandHandler = new AutoCadCommand("GEOINFO") });
            AddPanel(tab, "Civil 3D", new RibbonButton { Text = "Civil 3D Tools", ShowText = true, CommandHandler = new AutoCadCommand("GEOLOCAL") });
            AddPanel(tab, "Tools", new RibbonButton { Text = "GEOLOCAL", ShowText = true, CommandHandler = new AutoCadCommand("GEOLOCAL") });
            AddPanel(tab, "Settings", new RibbonButton { Text = "GeoLocal Settings", ShowText = true, CommandHandler = new AutoCadCommand("GEOSETTINGS") });
            ribbon.Tabs.Add(tab);
            _isCreated = true;
            PluginEntryPoint.Logger?.Information("GeoLocal Ribbon created with Phase 2 panels.");
        }

        private static void AddPanel(RibbonTab tab, string title, RibbonButton button)
        {
            var source = new RibbonPanelSource { Title = title };
            source.Items.Add(button);
            tab.Panels.Add(new RibbonPanel { Source = source });
        }

        private sealed class AutoCadCommand : ICommand
        {
            private readonly string _command;
            public AutoCadCommand(string command) { _command = command; }
            public bool CanExecute(object parameter) { return true; }
            public event EventHandler CanExecuteChanged { add { } remove { } }
            public void Execute(object parameter)
            {
                var document = Application.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    PluginEntryPoint.Logger?.Error("Ribbon command could not execute because there is no active drawing document.");
                    return;
                }
                PluginEntryPoint.Logger?.Information("Ribbon command invoked: " + _command);
                document.SendStringToExecute(_command + " ", true, false, false);
            }
        }
    }
}
