using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync_Assessment.Data;
using EduSync_Assessment.Models;
using EduSync_Assessment.DTO;
namespace EduSync_Assessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentTablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssessmentTablesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AssessmentTables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentReadDto>>> GetAssessmentTables()
        {
            var assessments = await _context.AssessmentTables.ToListAsync();

            var result = assessments.Select(a => new AssessmentReadDto
            {
                AssessmentId = a.AssessmentId,
                Title = a.Title,
                Questions = a.Questions,
                MaxScore = a.MaxScore,
                CourseId = a.CourseId
            });

            return Ok(result);
        }

        // GET: api/AssessmentTables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentReadDto>> GetAssessmentTable(Guid id)
        {
            var assessment = await _context.AssessmentTables.FindAsync(id);

            if (assessment == null)
                return NotFound();

            var dto = new AssessmentReadDto
            {
                AssessmentId = assessment.AssessmentId,
                Title = assessment.Title,
                Questions = assessment.Questions,
                MaxScore = assessment.MaxScore,
                CourseId = assessment.CourseId
            };

            return Ok(dto);
        }

        // POST: api/AssessmentTables
        [HttpPost]
        public async Task<ActionResult<AssessmentReadDto>> PostAssessmentTable(AssessmentCreateDto dto)
        {
            var assessment = new AssessmentTable
            {
                AssessmentId = Guid.NewGuid(),
                Title = dto.Title,
                Questions = dto.Questions,
                MaxScore = dto.MaxScore,
                CourseId = dto.CourseId
            };

            _context.AssessmentTables.Add(assessment);
            await _context.SaveChangesAsync();

            var result = new AssessmentReadDto
            {
                AssessmentId = assessment.AssessmentId,
                Title = assessment.Title,
                Questions = assessment.Questions,
                MaxScore = assessment.MaxScore,
                CourseId = assessment.CourseId
            };

            return CreatedAtAction(nameof(GetAssessmentTable), new { id = assessment.AssessmentId }, result);
        }

        // PUT: api/AssessmentTables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssessmentTable(Guid id, AssessmentUpdateDto dto)
        {
            var assessment = await _context.AssessmentTables.FindAsync(id);

            if (assessment == null)
                return NotFound();

            assessment.Title = dto.Title;
            assessment.Questions = dto.Questions;
            assessment.MaxScore = dto.MaxScore;

            _context.Entry(assessment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/AssessmentTables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessmentTable(Guid id)
        {
            var assessment = await _context.AssessmentTables.FindAsync(id);
            if (assessment == null)
                return NotFound();

            _context.AssessmentTables.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("by-instructor/{instructorId}")]
        public async Task<ActionResult<IEnumerable<AssessmentReadDto>>> GetAssessmentsByInstructor(string instructorId)
        {
            if (string.IsNullOrEmpty(instructorId))
            {
                return BadRequest("Instructor ID is required");
            }

            var assessments = await _context.AssessmentTables
                .Include(a => a.Course)
                .Where(a => a.Course.InstructorId.ToString() == instructorId)
                .Select(a => new AssessmentReadDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    Questions = a.Questions,
                    MaxScore = a.MaxScore,
                    CourseId = a.CourseId
                })
                .ToListAsync();

            return Ok(assessments);
        }

        private bool AssessmentTableExists(Guid id)
        {
            return _context.AssessmentTables.Any(e => e.AssessmentId == id);
        }
    }
}
