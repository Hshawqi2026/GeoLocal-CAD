using System;

namespace GeoLocalCAD.Core.Logging
{
    public interface IGeoLogger
    {
        void Log(LogLevel level, string message, Exception exception = null);
        void Debug(string message);
        void Information(string message);
        void Warning(string message);
        void Error(string message, Exception exception = null);
    }
}
