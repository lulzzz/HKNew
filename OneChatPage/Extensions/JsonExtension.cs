using System.Text.Json;

namespace OneChatPage.Extensions
{
    public static class JsonExtension
    {
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj);
        }
        public static T ToObject<T>(this string json)
        {
            //if (string.IsNullOrEmpty(json))
            //{
            //    return default(T);
            //}
            //return JsonSerializer.Deserialize<T>(json);
            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
