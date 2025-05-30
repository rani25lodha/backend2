using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace EduSync_Assessment.BlobServices
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<string> UploadUrlAsTextFileAsync(string fileName, string content);
        Task<string> GetFileContentAsync(string fileUrl);
        Task DeleteFileAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        Task<string> GetFileUrlAsync(string fileName);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"];

            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure container exists
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty");

            // Create a unique file name with timestamp to avoid conflicts
            string fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            BlobClient blobClient = _containerClient.GetBlobClient(fileName);

            // Set the content type based on the file extension
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(file.FileName, file.ContentType)
            };

            // Upload the file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Conditions = null // Allow overwrite if exists
                });
            }

            // Return the URL of the uploaded file
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadUrlAsTextFileAsync(string fileName, string content)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(content))
                throw new ArgumentException("FileName and content are required");

            BlobClient blobClient = _containerClient.GetBlobClient(fileName);

            // Set the content type for JSON
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/json"
            };

            // Convert string to byte array
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);

            // Upload the content as a blob
            using (var stream = new MemoryStream(contentBytes))
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Conditions = null // Allow overwrite if exists
                });
            }

            // Return the URL of the uploaded file
            return blobClient.Uri.ToString();
        }

        public async Task<string> GetFileContentAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return string.Empty;

            try
            {
                // Extract filename from URL if full URL is provided
                string fileName = fileUrl;
                if (fileUrl.StartsWith("https://"))
                {
                    var uri = new Uri(fileUrl);
                    fileName = Path.GetFileName(uri.LocalPath);
                }

                BlobClient blobClient = _containerClient.GetBlobClient(fileName);

                // Check if blob exists
                bool exists = await blobClient.ExistsAsync();
                if (!exists)
                    return string.Empty;

                // Download the blob content
                var response = await blobClient.DownloadContentAsync();
                return response.Value.Content.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            // Extract filename from URL if full URL is provided
            if (fileName.StartsWith("https://"))
            {
                var uri = new Uri(fileName);
                fileName = Path.GetFileName(uri.LocalPath);
            }

            BlobClient blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<bool> FileExistsAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            // Extract filename from URL if full URL is provided
            if (fileName.StartsWith("https://"))
            {
                var uri = new Uri(fileName);
                fileName = Path.GetFileName(uri.LocalPath);
            }

            BlobClient blobClient = _containerClient.GetBlobClient(fileName);
            var response = await blobClient.ExistsAsync();
            return response.Value;
        }

        public async Task<string> GetFileUrlAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            BlobClient blobClient = _containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        private string GetContentType(string fileName, string defaultContentType)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".avi" => "video/avi",
                ".mov" => "video/quicktime",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".json" => "application/json",
                ".txt" => "text/plain",
                _ => defaultContentType ?? "application/octet-stream"
            };
        }
    }
}