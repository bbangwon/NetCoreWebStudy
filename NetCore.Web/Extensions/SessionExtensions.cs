using Newtonsoft.Json;

namespace NetCore.Web.Extensions
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, List<T> values)
        {
            // JSON으로 직렬화하여 세션에 저장
            session.SetString(key, JsonConvert.SerializeObject(values));
        }

        public static void Set<T>(this ISession session, string key, T value)
        {
            // JSON으로 직렬화하여 세션에 저장
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            // 세션에서 문자열로 된 JSON을 가져옴
            var value = session.GetString(key);

            // 가져온 JSON을 역직렬화하여 객체로 변환하여 반환
            return (value != null)? JsonConvert.DeserializeObject<T>(value): default;
        }


    }
}
