using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Utils
{
    public static class HtmlHelpers
    {
        private class ScriptBlock : IDisposable
        {
            private const string scriptsKey = "scripts";

            public static List<string> pageScripts
            {
                get
                {
                    if (HttpContext.Current.Items[scriptsKey] == null)
                        HttpContext.Current.Items[scriptsKey] = new List<string>();
                    return (List<string>) HttpContext.Current.Items[scriptsKey];
                }
            }

            private readonly WebViewPage _webPageBase;

            public ScriptBlock(WebViewPage webPageBase)
            {
                this._webPageBase = webPageBase;
                this._webPageBase.OutputStack.Push(new StringWriter());
            }

            public void Dispose()
            {
                pageScripts.Add(_webPageBase.OutputStack.Pop().ToString());
            }
        }

        public static IDisposable BeginScripts(this HtmlHelper helper)
        {
            return new ScriptBlock((WebViewPage) helper.ViewDataContainer);
        }

        public static MvcHtmlString PageScripts(this HtmlHelper helper)
        {
            return
                MvcHtmlString.Create(string.Join(Environment.NewLine, ScriptBlock.pageScripts.Select(s => s.ToString())));
        }
    }
}
