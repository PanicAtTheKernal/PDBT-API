using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var result = await _userService.Login(request, Response);
            return result.Result;
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenDTO>> RefreshToken()
        {
            var result = await _userService.RefreshToken(Request, Response);
            return result.Result;
        }

        [HttpGet("{id}"), Authorize]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var result = await _userService.GetUserById(id);
            return result.Result;
        }
    }
}
