using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync_Assessment.Data;
using EduSync_Assessment.Models;
using EduSync_Assessment.DTO; // DTO namespace

namespace EduSync_Assessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserTablesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserTables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUserTables()
        {
            var users = await _context.UserTables
                .Select(u => new UserReadDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,  
                    PasswordHash = u.PasswordHash
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/UserTables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUserTable(Guid id)
        {
            var user = await _context.UserTables.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserReadDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(userDto);
        }

        // POST: api/UserTables
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUserTable(UserCreateDto dto)
        {
            var newUser = new UserTable
            {
                UserId = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = dto.PasswordHash
            };

            _context.UserTables.Add(newUser);
            await _context.SaveChangesAsync();

            var readDto = new UserReadDto
            {
                UserId = newUser.UserId,
                Name = newUser.Name,
                Email = newUser.Email,
                Role = newUser.Role
            };

            return CreatedAtAction(nameof(GetUserTable), new { id = newUser.UserId }, readDto);
        }

        // PUT: api/UserTables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserTable(Guid id, UserCreateDto dto)
        {
            var user = await _context.UserTables.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.PasswordHash = dto.PasswordHash;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/UserTables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTable(Guid id)
        {
            var user = await _context.UserTables.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.UserTables.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserTableExists(Guid id)
        {
            return _context.UserTables.Any(e => e.UserId == id);
        }
    }
}
