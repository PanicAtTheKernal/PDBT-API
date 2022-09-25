using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IUnitOfWork _context;
        public readonly IConfiguration _configuration;
        
        public UserController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _context = unitOfWork;
            _configuration = configuration;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegistration request)
        {
            // if (!(await _context.Users.GetAllAsync()).Any())
            // {
            //     return Problem("Entity set 'Users' is null");
            // }
            
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Email is already registered");
            }
            
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username is already registered");
            }

            CreatePasswordHash(request.Password, 
                out byte[] passwordHash,
                out byte[] passwordSalt);

            var user = new User()
            {
                Email = request.Email,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            
            _context.Users.Add(user);
            await _context.CompleteAsync();
            
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO request)
        {
            
            if (!await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("User does not exist");
            }

            var user = _context.Users.Find(u => u.Email == request.Email).FirstOrDefault();
            
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is wrong");
            }

            string token = CreateToken(user);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken, user.Id);

            await _context.CompleteAsync();
            
            return  new OkObjectResult(token);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenDTO>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userId = Request.Cookies["userId"];

            if (refreshToken == null)
            {
                return Unauthorized("Refresh Token missing");
            }
            
            User user = await _context.Users.GetByIdAsync(int.Parse(userId!));

            if (!user.RefreshToken!.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token");
            }
            
            if(user.RefreshTokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken, user.Id);
            var response = new RefreshTokenDTO()
            {
                JWT = token,
                Token = newRefreshToken.Token,
                Expries = newRefreshToken.Expries
            };

            await _context.CompleteAsync();

            return Ok(response);
        }
        
        private async void  SetRefreshToken(RefreshToken refreshToken, int userId)
        {
            var cookieOptions = new CookieOptions
            {
                IsEssential = true,
                Secure = false,
                HttpOnly = false,
                Expires = refreshToken.Expries
            };
            
            Response.Cookies.Append("refreshToken", refreshToken.Token,cookieOptions);
            Response.Cookies.Append("userId", userId.ToString(), cookieOptions);

            var user = await _context.Users.GetByIdAsync(userId);
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenCreated = refreshToken.Created;
            user.RefreshTokenExpires = refreshToken.Expries;

            await _context.Users.Update(user);
            
        }

        private RefreshToken GenerateRefreshToken() => 
            new()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expries = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
        

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            };
            
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("jwttoken:key").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            
            return jwt;
        }
    }
}
