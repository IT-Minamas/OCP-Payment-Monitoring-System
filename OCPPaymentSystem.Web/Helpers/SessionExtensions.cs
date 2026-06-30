using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace OCPPaymentSystem.Web.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(
            this ISession session,
            string key,
            T value)
        {
            session.SetString(
                key,
                JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(
            this ISession session,
            string key)
        {
            string? json =
                session.GetString(key);

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}