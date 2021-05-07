using Microsoft.Extensions.Logging;

namespace EasyConnect.Common
{
    public class Logging
    {
        private static LoggerFactory _factory;

        static Logging()
        {
            _factory = new LoggerFactory();
#if DEBUG
            _factory.AddProvider(new DebugLoggerProvider());
#endif
        }

        public static LoggerFactory Factory
        {
            get
            {
                return _factory;
            }
        }

        private Logging()
        {
        }
    }
}
