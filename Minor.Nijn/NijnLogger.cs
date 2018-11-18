using Microsoft.Extensions.Logging;

namespace Minor.Nijn
{
    public static class NijnLogger
    {
        private static ILoggerFactory DefaultFactory { get; set; }  = new LoggerFactory();
        public static ILoggerFactory LoggerFactory { get; set; }
        public static ILogger CreateLogger<T>()
        {
            if (LoggerFactory == null) return DefaultFactory.CreateLogger<T>();
            return LoggerFactory.CreateLogger<T>();
        }
    }
}
