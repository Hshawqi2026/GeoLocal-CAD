using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace GeoLocalCAD.Core.Logging
{
    public sealed class FileGeoLogger : IGeoLogger, IDisposable
    {
        private readonly object _sync = new object();
        private readonly string _logFilePath;
        private bool _disposed;

        public FileGeoLogger(string applicationName = "GeoLocalCAD")
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDirectory = Path.Combine(baseDirectory, applicationName, "Logs");
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, applicationName + ".log");
        }

        public string LogFilePath { get { return _logFilePath; } }

        public void Log(LogLevel level, string message, Exception exception = null)
        {
            if (_disposed) return;
            var builder = new StringBuilder();
            builder.Append(DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            builder.Append(" [").Append(level.ToString().ToUpperInvariant()).Append("] ");
            builder.Append(message ?? string.Empty);
            if (exception != null)
            {
                builder.AppendLine();
                builder.Append(exception);
            }
            lock (_sync)
            {
                File.AppendAllText(_logFilePath, builder + Environment.NewLine, Encoding.UTF8);
            }
        }

        public void Debug(string message) { Log(LogLevel.Debug, message); }
        public void Information(string message) { Log(LogLevel.Information, message); }
        public void Warning(string message) { Log(LogLevel.Warning, message); }
        public void Error(string message, Exception exception = null) { Log(LogLevel.Error, message, exception); }
        public void Dispose() { _disposed = true; }
    }
}
