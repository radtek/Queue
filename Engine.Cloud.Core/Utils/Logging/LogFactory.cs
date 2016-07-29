using System;

namespace Utils
{
    public static class LogFactory
    {
        private static ILogger _logger;


        public static void Init()
        {
            _logger = new Log4NetAdapter();
        }

        public static ILogger GetInstance()
        {
            if (_logger == null)
                Init();

            return _logger;
        }
    }

    public interface ILogger
    {
        void Log(string message, bool error = false);
        void Log(Exception ex);
        void Log(string message, Exception ex);
    }
}
