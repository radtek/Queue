using Newtonsoft.Json;

namespace System
{
    public static class JsonExtension
    {
        public static T DeserializeObject<T>(string value)
        {
            value = value.Replace(@"\", " ");
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
