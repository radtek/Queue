using System;
using log4net;

namespace Utils
{
    public class Log4NetAdapter : ILogger
    {
        private readonly ILog _log;

        public Log4NetAdapter()
        {
            _log = LogManager.GetLogger(typeof(Log4NetAdapter));
        }

        public void Log(string message, bool error = false)
        {
            if (!error)
                _log.Info(message);
            else
            {
                _log.Error(message);
            }
        }

        public void Log(Exception e)
        {
            _log.Error(e);
        }

        public void Log(string message, Exception e)
        {
            _log.Error(message, e);
        }
    }
}