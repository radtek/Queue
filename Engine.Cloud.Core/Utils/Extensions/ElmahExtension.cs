using System;
using System.Web;
using Elmah;

namespace Engine.Cloud.Core.Utils.Extensions
{
    public static class ElmahExtension
    {
        public static void LogToElmah(this Exception ex)
        {
            if (HttpContext.Current != null)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            else
            {
                if (httpApplication == null)
                    InitNoContext();
                ErrorSignal.Get(httpApplication).Raise(ex);
            }
        }
        public static void LogToElmah(this Exception ex, string message)
        {
            var exception = new Exception(message, ex);

            if (HttpContext.Current != null)
            {
                ErrorSignal.FromCurrentContext().Raise(exception.InnerException);
            }
            else
            {
                if (httpApplication == null)
                    InitNoContext();
                ErrorSignal.Get(httpApplication).Raise(exception);
            }
        }

        private static HttpApplication httpApplication = null;
        private static ErrorFilterConsole errorFilter = new ErrorFilterConsole();

        public static ErrorMailModule ErrorEmail = new ErrorMailModule();
        public static ErrorLogModule ErrorLog = new ErrorLogModule();
        public static ErrorTweetModule ErrorTweet = new ErrorTweetModule();

        private static void InitNoContext()
        {
            httpApplication = new HttpApplication();
            errorFilter.Init(httpApplication);

            (ErrorEmail as IHttpModule).Init(httpApplication);
            errorFilter.HookFiltering(ErrorEmail);

            (ErrorLog as IHttpModule).Init(httpApplication);
            errorFilter.HookFiltering(ErrorLog);

            (ErrorTweet as IHttpModule).Init(httpApplication);
            errorFilter.HookFiltering(ErrorTweet);
        }

        private class ErrorFilterConsole : ErrorFilterModule
        {
            public void HookFiltering(IExceptionFiltering module)
            {
                module.Filtering += new ExceptionFilterEventHandler(base.OnErrorModuleFiltering);
            }
        }
    }
}