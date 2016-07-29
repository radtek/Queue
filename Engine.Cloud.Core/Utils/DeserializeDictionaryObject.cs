using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Engine.Cloud.Core.Utils
{
    public class DeserializeDictionaryObject
    {
        public static Dictionary<string, object> DeserializeToDictionary(string json)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var result = new Dictionary<string, object>();
            foreach (var item in values)
            {
                if (item.Value is JObject)
                {
                    result.Add(item.Key, DeserializeToDictionary(item.Value.ToString()));
                }
                else
                {
                    result.Add(item.Key, item.Value);
                }
            }
            return result;
        }
    }
}
