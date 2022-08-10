using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IUnitOfWork _context;
        
        public UserController(IUnitOfWork unitOfWork)
        {
            _context = unitOfWork;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegistration request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Email is already registered");
            }
            
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username is already registered");
            }

            return NoContent();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
