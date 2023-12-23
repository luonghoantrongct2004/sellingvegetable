using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace FruityFresh.Extension
{
    public static class SessionExtention
    {
        public static void SetObjectFromJson<T>(this ISession session,string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
