using System;
using System.Windows;
using GeoLocalCAD.Core.Logging;
using GeoLocalCAD.Core.Settings;

namespace GeoLocalCAD.UI
{
    public partial class GeoSettingsWindow : Window
    {
        private readonly GeoSettingsStore _store;
        private readonly IGeoLogger _logger;

        public GeoSettingsWindow(GeoSettingsStore store, IGeoLogger logger)
        {
            InitializeComponent();
            _store = store;
            _logger = logger;
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var settings = _store.Load();
                OfflineModeCheckBox.IsChecked = settings.OfflineMode;
                DrawingCrsTextBox.Text = settings.DrawingCrs ?? string.Empty;
                StatusTextBlock.Text = "Settings are stored locally at:\n" + _store.SettingsPath;
            }
            catch (Exception exception)
            {
                _logger.Error("Settings window could not load settings.", exception);
                StatusTextBlock.Text = "Could not load settings. The technical details were written to the GeoLocal CAD log.";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = _store.Load();
                settings.OfflineMode = OfflineModeCheckBox.IsChecked == true;
                settings.DrawingCrs = (DrawingCrsTextBox.Text ?? string.Empty).Trim();
                _store.Save(settings);
                _logger.Information("GeoLocal CAD settings saved. OfflineMode=" + settings.OfflineMode + ", DrawingCRS=" + settings.DrawingCrs);
                DialogResult = true;
                Close();
            }
            catch (Exception exception)
            {
                _logger.Error("Settings window could not save settings.", exception);
                MessageBox.Show(
                    "Settings could not be saved. Verify that the local application data folder is writable, then try again.\n\nTechnical details are in the GeoLocal CAD log.",
                    "GeoLocal CAD",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
