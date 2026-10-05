using System;
using System.Windows;
using GeoLocalCAD.Core.Inspection;
using GeoLocalCAD.Core.Logging;

namespace GeoLocalCAD.UI
{
    public partial class CoordinateInspectorWindow : Window
    {
        private readonly CoordinateInspectionResult _result;
        private readonly IGeoLogger _logger;
        public CoordinateInspectorWindow(CoordinateInspectionResult result, IGeoLogger logger)
        {
            InitializeComponent(); _result = result; _logger = logger; RenderResult();
        }
        private void RenderResult()
        {
            EntityTextBlock.Text = "Entity: " + _result.Observation.EntityType;
            DrawingCoordinateTextBlock.Text = _result.FormatDrawingCoordinate();
            GeographicCoordinateTextBlock.Text = _result.FormatGeographicCoordinate();
            DetailsTextBox.Text = "Status: " + _result.Status + Environment.NewLine + (string.IsNullOrWhiteSpace(_result.Warning) ? "" : "Warning: " + _result.Warning);
        }
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try { Clipboard.SetText(_result.FormatDrawingCoordinate() + Environment.NewLine + _result.FormatGeographicCoordinate()); _logger?.Information("GEOINFO coordinates copied to clipboard."); DetailsTextBox.Text += Environment.NewLine + "Copied to clipboard."; }
            catch (Exception exception) { _logger?.Error("GEOINFO could not copy coordinates to clipboard.", exception); MessageBox.Show("Coordinates could not be copied. Review the GeoLocal CAD log.", "GeoLocal CAD", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}
