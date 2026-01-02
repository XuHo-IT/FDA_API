using FDAAPI.App.Common.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Services.Auth
{
    public class ImageKitService : IImageStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _urlEndpoint;
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _uploadEndpoint;
        private readonly string _deleteEndpoint;

        public ImageKitService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _urlEndpoint = configuration["ImageKit:UrlEndpoint"] ?? string.Empty;
            _publicKey = configuration["ImageKit:PublicKey"] ?? string.Empty;
            _privateKey = configuration["ImageKit:PrivateKey"] ?? string.Empty;
            _uploadEndpoint = configuration["ImageKit:UploadEndpoint"] ?? string.Empty;
            _deleteEndpoint = configuration["ImageKit:DeleteEndpoint"] ?? string.Empty;
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "products")
        {
            try
            {
                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(imageStream);
                content.Add(fileContent, "file", fileName);
                content.Add(new StringContent(fileName), "fileName");
                if (!string.IsNullOrEmpty(folder)) content.Add(new StringContent(folder), "folder");

                var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_privateKey}:"));
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);

                var response = await _httpClient.PostAsync(_uploadEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"ImageKit upload failed with status {response.StatusCode}: {responseContent}");

                var uploadResult = JsonSerializer.Deserialize<ImageKitUploadResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (uploadResult == null)
                    throw new Exception("ImageKit upload succeeded but returned null response.");

                if (!string.IsNullOrEmpty(uploadResult.FilePath))
                {
                    return string.Concat(_urlEndpoint.TrimEnd('/'), uploadResult.FilePath);
                }

                return uploadResult.Url ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image to ImageKit: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteImageAsync(string imageId)
        {
            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var token = GenerateToken(imageId, timestamp);

                var deleteData = new
                {
                    fileId = imageId,
                    publicKey = _publicKey,
                    timestamp = timestamp,
                    token = token
                };

                var json = JsonSerializer.Serialize(deleteData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_deleteEndpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting image from ImageKit: {ex.Message}", ex);
            }
        }

        public Task<string> GetImageUrlAsync(string imagePathOrUrl, int? width = null, int? height = null)
        {
            if (string.IsNullOrWhiteSpace(imagePathOrUrl)) return Task.FromResult(string.Empty);

            string finalUrl;

            if (imagePathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || imagePathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                finalUrl = Regex.Replace(imagePathOrUrl, @"https:\/\/[^\/]+\/", $"{_urlEndpoint.TrimEnd('/')}/");
            }
            else
            {
                finalUrl = string.Concat(_urlEndpoint.TrimEnd('/'), "/", imagePathOrUrl.TrimStart('/'));
            }

            if (width.HasValue || height.HasValue)
            {
                var transformations = new List<string>();
                if (width.HasValue) transformations.Add($"w-{width}");
                if (height.HasValue) transformations.Add($"h-{height}");

                var separator = finalUrl.Contains('?') ? '&' : '?';

                if (finalUrl.Contains("tr="))
                {
                    finalUrl = Regex.Replace(finalUrl, @"tr=([^&]*)", match => $"tr={match.Groups[1].Value},{string.Join(",", transformations)}");
                }
                else
                {
                    finalUrl += $"{separator}tr={string.Join(",", transformations)}";
                }
            }

            return Task.FromResult(finalUrl);
        }

        public Task<string> GetTransformedImageUrlAsync(string imageId, string transformation)
        {
            var url = string.Concat(_urlEndpoint.TrimEnd('/'), "/", imageId, "?tr=", transformation);
            return Task.FromResult(url);
        }

        private string GenerateToken(string fileName, long timestamp)
        {
            var message = $"{timestamp}{_publicKey}{fileName}";
            using var hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(_privateKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
