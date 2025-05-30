using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync_Assessment.Data;
using EduSync_Assessment.Models;
using EduSync_Assessment.DTO;
using EduSync_Assessment.BlobServices;

namespace EduSync_Assessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseTablesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IBlobStorageService _blobStorageService;
        public CourseTablesController(AppDbContext context, IBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }
        //course-media of course table is stored in Azure Blob Storage, so we need to inject the blob storage service to handle media uploads.
        [HttpPost("upload")]
        public async Task<IActionResult> UploadCourseMedia(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file was provided." });

                // Validate file size (50MB limit)
                const long maxFileSize = 50 * 1024 * 1024; // 50MB
                if (file.Length > maxFileSize)
                    return BadRequest(new { error = "File size exceeds 50MB limit." });

                // Validate file type for course media
                var allowedTypes = new[] {
            "image/jpeg", "image/jpg", "image/png", "image/gif",
            "video/mp4", "video/avi", "video/mov", "video/quicktime",
            "application/pdf"
        };

                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(new
                    {
                        error = "Invalid file type for course media.",
                        allowedTypes = new[] { "JPEG", "PNG", "GIF", "MP4", "AVI", "MOV", "PDF" }
                    });

                // Upload file to Azure Blob Storage using the injected service
                string fileUrl = await _blobStorageService.UploadFileAsync(file);

                return Ok(new
                {
                    success = true,
                    url = fileUrl,
                    fileName = file.FileName,
                    fileSize = file.Length,
                    contentType = file.ContentType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to upload course media",
                    details = ex.Message
                });
            }
        }
        // GET: api/CourseTables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseReadDto>>> GetCourseTables()
        {
            var courses = await _context.CourseTables.ToListAsync();

            var result = courses.Select(c => new CourseReadDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                MediaUrl = c.MediaUrl
            });

            return Ok(result);
        }

        // GET: api/CourseTables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseReadDto>> GetCourseTable(Guid id)
        {
            var course = await _context.CourseTables.FindAsync(id);

            if (course == null)
                return NotFound();

            var result = new CourseReadDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            return Ok(result);
        }

        // POST: api/CourseTables
        [HttpPost]
        public async Task<ActionResult<CourseReadDto>> PostCourseTable(CourseCreateDto dto)
        {
            var course = new CourseTable
            {
                CourseId = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                InstructorId = dto.InstructorId,
                MediaUrl = dto.MediaUrl
            };

            _context.CourseTables.Add(course);
            await _context.SaveChangesAsync();

            var result = new CourseReadDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            return CreatedAtAction(nameof(GetCourseTable), new { id = course.CourseId }, result);
        }

        // PUT: api/CourseTables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseTable(Guid id, CourseUpdateDto dto)
        {
            var course = await _context.CourseTables.FindAsync(id);

            if (course == null)
                return NotFound();

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.MediaUrl = dto.MediaUrl;

            _context.Entry(course).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CourseTables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseTable(Guid id)
        {
            var course = await _context.CourseTables.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.CourseTables.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/CourseTables/by-instructor/123
        [HttpGet("by-instructor/{instructorId}")]
        public async Task<ActionResult<IEnumerable<CourseReadDto>>> GetCoursesByInstructor(Guid instructorId)
        {
            var courses = await _context.CourseTables
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();

            var result = courses.Select(c => new CourseReadDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                MediaUrl = c.MediaUrl
            });

            return Ok(result);
        }

        
    }
}
