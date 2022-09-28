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
using PDBT.Services.UserService;

namespace PDBT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegistration request)
        {
            var result = await _userService.Register(request);
            return result.Result;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO request)
        {
            var response = await _userService.Login(request);
            return response.Result;
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
        

    }
}
