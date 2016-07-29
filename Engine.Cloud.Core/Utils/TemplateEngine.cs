using System.Collections.Generic;
using Utils;

namespace Engine.Cloud.Core.Utils
{
    public static class TemplateEngine
    {
        public static string Build(Dictionary<string, string> keys, string input)
        {
            const string startToken = "<%=:";
            const string endToken = "%>";
            var replacer = new FastReplacer(startToken, endToken, false);

            replacer.Append(input);

            foreach (var item in keys)
            {
                replacer.Replace(startToken + item.Key + endToken, item.Value);
            }

            return replacer.ToString();
        }
    }
}
