using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OCPPaymentSystemAPI.Helpers
{
    public static class AppEnvironment
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        private static string PRDBaseUrl = "";
        private static string QASBaseUrl = "";

        public static void Initialize(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            PRDBaseUrl = (configuration["AppSettings:BaseUrls:PRD"] ?? "").TrimEnd('/');
            QASBaseUrl = (configuration["AppSettings:BaseUrls:QAS"] ?? "").TrimEnd('/');
        }

        public static string GetBaseUrl()
        {
            var request = _httpContextAccessor?.HttpContext?.Request;
            return request == null ? "" : $"{request.Scheme}://{request.Host}".TrimEnd('/');
        }

        public static bool IsProductionServer()
        {
            return IsMatchingHost(PRDBaseUrl);
        }

        public static bool IsQASServer()
        {
            var request = _httpContextAccessor?.HttpContext?.Request;
            string host = request?.Host.Host ?? "";

            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("::1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return IsMatchingHost(QASBaseUrl);
        }

        private static bool IsMatchingHost(string configuredUrl)
        {
            var request = _httpContextAccessor?.HttpContext?.Request;
            return request != null
                && Uri.TryCreate(configuredUrl, UriKind.Absolute, out var configuredUri)
                && request.Host.Host.Equals(configuredUri.Host, StringComparison.OrdinalIgnoreCase);
        }
    }
}