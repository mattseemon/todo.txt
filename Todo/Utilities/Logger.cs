using Rareform.Validation;
using LogLevel = Splat.LogLevel;

namespace Seemon.Todo.Utilities
{
    public class Logger : Splat.ILogger
    {
        private readonly NLog.Logger logger;

        public Logger(NLog.Logger logger)
        {
            if (logger == null)
                Throw.ArgumentNullException(() => logger);

            this.logger = logger;
        }

        public NLog.Logger NLogger { get { return this.logger; } }

        public LogLevel Level { get; set; }

        public void Write(string message, LogLevel logLevel)
        {
            this.logger.Log(NLogLevelToSplatLogLevel(logLevel), message);
        }

        private static NLog.LogLevel NLogLevelToSplatLogLevel(Splat.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Fatal:
                    return NLog.LogLevel.Fatal;
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Warn:
                    return NLog.LogLevel.Warn;
            }

            return NLog.LogLevel.Off;
        }
    }
}
