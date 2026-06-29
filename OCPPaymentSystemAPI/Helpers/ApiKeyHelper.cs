namespace OCPPaymentSystemAPI.Helpers
{
    public static class ApiKeyHelper
    {
        public static bool Validate(
            IConfiguration configuration,
            string apiKey)
        {
            string validApiKey =
                configuration["ApiKey"] ?? "";

            return apiKey == validApiKey;
        }
    }
}
