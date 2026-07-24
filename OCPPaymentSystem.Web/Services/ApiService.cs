using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using OCPPaymentSystem.Web.Models;

namespace OCPPaymentSystem.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;

        public ApiService(
            HttpClient client,
            IConfiguration config,
            IHttpContextAccessor httpContext)
        {
            _client = client;
            _config = config;
            _httpContext = httpContext;
        }

        private string BaseUrl =>
            _config["ApiSetting:BaseUrl"]!;

        private void AddApiKey(string apiKey)
        {
            _client.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _client.DefaultRequestHeaders.Add(
                    "x-api-key",
                    apiKey);
            }
        }

        public async Task<MemoAttachmentDownload> GetFileAsync(
    string url,
    bool useApiKey,
    string apiKey)
        {

            if (useApiKey)
            {
                _client.DefaultRequestHeaders.Remove(
                    "x-api-key");


                _client.DefaultRequestHeaders.Add(
                    "x-api-key",
                    apiKey);
            }


            var response =
                await _client.GetAsync(
                    BaseUrl + url);


            byte[] data =
                await response.Content
                .ReadAsByteArrayAsync();


            string fileName =
                response.Content.Headers
                .ContentDisposition?
                .FileName?
                .Replace("\"", "")
                ?? "attachment";


            string contentType =
                response.Content.Headers
                .ContentType?
                .ToString()
                ?? "application/octet-stream";


            return new MemoAttachmentDownload
            {
                FileName = fileName,
                ContentType = contentType,
                FileData = data
            };

        }

        public async Task<TResponse?> PostFileAsync<TResponse>(
            string url,
            MultipartFormDataContent content,
            bool useApiKey,
            string apiKey)
        {

            if (useApiKey)
            {
                _client.DefaultRequestHeaders.Remove(
                    "x-api-key");


                _client.DefaultRequestHeaders.Add(
                    "x-api-key",
                    apiKey);
            }


            var response =
                await _client.PostAsync(
                    BaseUrl + url,
                    content);


            string json =
                await response.Content
                .ReadAsStringAsync();


            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    "API ERROR : " +
                    response.StatusCode +
                    " - " +
                    json);
            }


            return JsonSerializer.Deserialize<TResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        }
        //-------------------------------------------------------
        // POST
        //-------------------------------------------------------

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            bool useApiKey = true,
            string? overrideApiKey = null)
        {
            if (useApiKey)
            {
                if (string.IsNullOrWhiteSpace(overrideApiKey))
                    AddApiKey("");
                else
                    AddApiKey(overrideApiKey);
            }

            string json =
                JsonSerializer.Serialize(request);

            StringContent content =
                new(
                    json,
                    Encoding.UTF8,
                    "application/json");

            HttpResponseMessage response =
                await _client.PostAsync(
                    BaseUrl + endpoint,
                    content);

            string result =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Status : {(int)response.StatusCode}\n\n{result}");
            }

            return JsonSerializer.Deserialize<TResponse>(
                result,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<bool> UploadFileAsync(
            string endpoint,
            IFormFile file,
            Dictionary<string, string> fields,
            bool useApiKey = true,
            string? overrideApiKey = null)
        {
            if (useApiKey)
            {
                if (string.IsNullOrWhiteSpace(overrideApiKey))
                    AddApiKey("");
                else
                    AddApiKey(overrideApiKey);
            }

            using MultipartFormDataContent form = new();

            foreach (var item in fields)
            {
                form.Add(
                    new StringContent(item.Value),
                    item.Key);
            }

            using StreamContent fileContent =
                new StreamContent(file.OpenReadStream());

            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType);

            form.Add(
                fileContent,
                "File",
                file.FileName);

            HttpResponseMessage response =
                await _client.PostAsync(
                    BaseUrl + endpoint,
                    form);

            return response.IsSuccessStatusCode;
        }

        //-------------------------------------------------------
        // GET
        //-------------------------------------------------------

        public async Task<TResponse?> GetAsync<TResponse>(
            string endpoint,
            bool useApiKey = true,
            string? overrideApiKey = null)
        {
            if (useApiKey)
            {
                if (string.IsNullOrWhiteSpace(overrideApiKey))
                    AddApiKey("");
                else
                    AddApiKey(overrideApiKey);
            }

            HttpResponseMessage response =
                await _client.GetAsync(
                    BaseUrl + endpoint);

            string result =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Status : {(int)response.StatusCode}\n\n{result}");
            }

            return JsonSerializer.Deserialize<TResponse>(
                result,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        //-------------------------------------------------------
        // DELETE
        //-------------------------------------------------------

        public async Task<bool> DeleteAsync(
            string endpoint,
            bool useApiKey = true,
            string? overrideApiKey = null)
        {
            if (useApiKey)
            {
                if (string.IsNullOrWhiteSpace(overrideApiKey))
                    AddApiKey("");
                else
                    AddApiKey(overrideApiKey);
            }

            HttpResponseMessage response =
                await _client.DeleteAsync(
                    BaseUrl + endpoint);

            return response.IsSuccessStatusCode;
        }

        public async Task<HttpResponseMessage>
        DownloadAsync(
            string endpoint,
            bool useApiKey = true,
            string? overrideApiKey = null
            )
        {
            if (useApiKey)
            {
                if (string.IsNullOrWhiteSpace(overrideApiKey))
                    AddApiKey("");
                else
                    AddApiKey(overrideApiKey);
            }

            return await _client.GetAsync(
                BaseUrl + endpoint);
        }

    }
}