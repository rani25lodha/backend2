//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using EduSync_Assessment.BlobServices;


//namespace EduSync_Assessment.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FileController : ControllerBase
//    {
//        private readonly IBlobStorageService _blobStorageService;

//        public FileController(IBlobStorageService blobStorageService)
//        {
//            _blobStorageService = blobStorageService;
//        }

//        [HttpPost("upload")]
//        public async Task<IActionResult> UploadFile(IFormFile file)
//        {
//            try
//            {
//                if (file == null || file.Length == 0)
//                    return BadRequest(new { error = "No file was provided." });

//                // Validate file size (50MB limit)
//                const long maxFileSize = 50 * 1024 * 1024; // 50MB
//                if (file.Length > maxFileSize)
//                    return BadRequest(new { error = "File size exceeds 50MB limit." });

//                // Validate file type
//                var allowedTypes = new[] {
//                    "image/jpeg", "image/jpg", "image/png", "image/gif",
//                    "video/mp4", "video/avi", "video/mov", "video/quicktime",
//                    "application/pdf", "application/msword",
//                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
//                };

//                if (!allowedTypes.Contains(file.ContentType.ToLower()))
//                    return BadRequest(new
//                    {
//                        error = "Invalid file type. Only image, video, and document files are allowed.",
//                        allowedTypes = new[] { "JPEG", "PNG", "GIF", "MP4", "AVI", "MOV", "PDF", "DOC", "DOCX" }
//                    });

//                // Upload file to Azure Blob Storage
//                string fileUrl = await _blobStorageService.UploadFileAsync(file);

//                return Ok(new
//                {
//                    success = true,
//                    url = fileUrl,
//                    fileName = file.FileName,
//                    fileSize = file.Length,
//                    contentType = file.ContentType
//                });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new
//                {
//                    success = false,
//                    error = "Failed to upload file",
//                    details = ex.Message
//                });
//            }
//        }

//        [HttpDelete("delete")]
//        public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(fileUrl))
//                    return BadRequest(new { error = "File URL is required." });

//                // Check if file exists
//                bool exists = await _blobStorageService.FileExistsAsync(fileUrl);
//                if (!exists)
//                    return NotFound(new { error = "File not found." });

//                // Delete the file
//                await _blobStorageService.DeleteFileAsync(fileUrl);

//                return Ok(new
//                {
//                    success = true,
//                    message = "File deleted successfully."
//                });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new
//                {
//                    success = false,
//                    error = "Failed to delete file",
//                    details = ex.Message
//                });
//            }
//        }

//        [HttpGet("exists")]
//        public async Task<IActionResult> CheckFileExists([FromQuery] string fileUrl)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(fileUrl))
//                    return BadRequest(new { error = "File URL is required." });

//                bool exists = await _blobStorageService.FileExistsAsync(fileUrl);

//                return Ok(new
//                {
//                    exists = exists,
//                    url = fileUrl
//                });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new
//                {
//                    error = "Failed to check file existence",
//                    details = ex.Message
//                });
//            }
//        }
//    }
//}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduSync_Assessment.BlobServices;
using System.Text;

namespace EduSync_Assessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public FileController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file was provided." });

                // Validate file size (50MB limit)
                const long maxFileSize = 50 * 1024 * 1024; // 50MB
                if (file.Length > maxFileSize)
                    return BadRequest(new { error = "File size exceeds 50MB limit." });

                // Validate file type
                var allowedTypes = new[] {
                    "image/jpeg", "image/jpg", "image/png", "image/gif",
                    "video/mp4", "video/avi", "video/mov", "video/quicktime",
                    "application/pdf", "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                };

                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(new
                    {
                        error = "Invalid file type. Only image, video, and document files are allowed.",
                        allowedTypes = new[] { "JPEG", "PNG", "GIF", "MP4", "AVI", "MOV", "PDF", "DOC", "DOCX" }
                    });

                // Upload file to Azure Blob Storage
                string fileUrl = await _blobStorageService.UploadFileAsync(file);

                return Ok(new
                {
                    success = true,
                    url = fileUrl,
                    fileName = file.FileName,
                    fileSize = file.Length,
                    contentType = file.ContentType,
                    type = "file"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to upload file",
                    details = ex.Message
                });
            }
        }

        // New endpoint to store URL links in Azure Blob Storage
        [HttpPost("upload-url")]
        public async Task<IActionResult> UploadUrl([FromBody] UrlUploadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Url))
                    return BadRequest(new { error = "URL is required." });

                // Validate URL format
                if (!Uri.TryCreate(request.Url, UriKind.Absolute, out Uri validatedUri))
                    return BadRequest(new { error = "Invalid URL format." });

                // Create a text file containing the URL and metadata
                var urlData = new
                {
                    originalUrl = request.Url,
                    title = request.Title ?? "External Media Link",
                    uploadedAt = DateTime.UtcNow,
                    type = "url-link"
                };

                string jsonContent = System.Text.Json.JsonSerializer.Serialize(urlData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Create a unique filename for the URL reference
                string fileName = $"url-link_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.json";

                // Upload the URL data as a JSON file to Azure Blob Storage
                string blobUrl = await _blobStorageService.UploadUrlAsTextFileAsync(fileName, jsonContent);

                return Ok(new
                {
                    success = true,
                    url = blobUrl,
                    originalUrl = request.Url,
                    fileName = fileName,
                    contentType = "application/json",
                    type = "url"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to store URL",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return BadRequest(new { error = "File URL is required." });

                // Check if file exists
                bool exists = await _blobStorageService.FileExistsAsync(fileUrl);
                if (!exists)
                    return NotFound(new { error = "File not found." });

                // Delete the file
                await _blobStorageService.DeleteFileAsync(fileUrl);

                return Ok(new
                {
                    success = true,
                    message = "File deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to delete file",
                    details = ex.Message
                });
            }
        }

        [HttpGet("exists")]
        public async Task<IActionResult> CheckFileExists([FromQuery] string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return BadRequest(new { error = "File URL is required." });

                bool exists = await _blobStorageService.FileExistsAsync(fileUrl);

                return Ok(new
                {
                    exists = exists,
                    url = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to check file existence",
                    details = ex.Message
                });
            }
        }

        // Get the original URL from a stored URL reference
        [HttpGet("get-original-url")]
        public async Task<IActionResult> GetOriginalUrl([FromQuery] string blobUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(blobUrl))
                    return BadRequest(new { error = "Blob URL is required." });

                string content = await _blobStorageService.GetFileContentAsync(blobUrl);

                if (string.IsNullOrEmpty(content))
                    return NotFound(new { error = "URL reference not found." });

                var urlData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(content);

                return Ok(new
                {
                    success = true,
                    originalUrl = urlData.GetProperty("originalUrl").GetString(),
                    title = urlData.GetProperty("title").GetString(),
                    uploadedAt = urlData.GetProperty("uploadedAt").GetDateTime()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to retrieve original URL",
                    details = ex.Message
                });
            }
        }
    }

    // Request model for URL uploads
    public class UrlUploadRequest
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }
}