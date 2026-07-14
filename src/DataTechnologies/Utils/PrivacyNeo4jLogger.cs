using System;
using Neo4j.Driver;

namespace DataTechnologies.Utils
{
    public class PrivacyNeo4jLogger : INeo4jLogger
    {
        public void Debug(string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message)) return;

            string formattedMessage = args != null && args.Length > 0 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args) : message;

            if (formattedMessage.Contains("parameters=", StringComparison.Ordinal))
            {
                formattedMessage = formattedMessage.Split("parameters=")[0] + "[PARAMETERS REDACTED FOR PRIVACY]";
            }

            MemoryLoggerProvider.AddLog("Neo4j", "Debug", "Neo4j.Driver", formattedMessage);
        }

        public void Info(string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message)) return;
            string formattedMessage = args != null && args.Length > 0 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args) : message;
            MemoryLoggerProvider.AddLog("Neo4j", "Info", "Neo4j.Driver", formattedMessage);
        }

        public void Warn(Exception cause, string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message)) return;
            string formattedMessage = args != null && args.Length > 0 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args) : message;
            if (cause != null)
            {
                formattedMessage += " | Exception: " + cause.Message;
            }
            MemoryLoggerProvider.AddLog("Neo4j", "Warning", "Neo4j.Driver", formattedMessage);
        }

        public void Error(Exception cause, string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message)) return;
            string formattedMessage = args != null && args.Length > 0 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args) : message;
            if (cause != null)
            {
                formattedMessage += " | Exception: " + cause.Message;
            }
            MemoryLoggerProvider.AddLog("Neo4j", "Error", "Neo4j.Driver", formattedMessage);
        }

        public void Trace(string message, params object[] args)
        {
        }

        public bool IsTraceEnabled() => false;
        public bool IsDebugEnabled() => true;
    }
}
