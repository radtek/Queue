using Newtonsoft.Json;

namespace Utils
{
    public abstract class SerializableJsonObject <T>
    {
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public T Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }
    }
}