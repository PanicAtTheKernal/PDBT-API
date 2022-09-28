using Microsoft.AspNetCore.Mvc;
using PDBT.Models;
using PDBT.Repository;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace PDBT.Services.UserService;

public class UserService: IUserService
{
    private readonly IUnitOfWork _context;
    private readonly IConfiguration _configuration;

    public UserService(IUnitOfWork context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ServiceResponse<User>> Register(UserRegistration registrationRequest)
    {
        var response = new ServiceResponse<User>();

        if (await IsUsernameRegistered(registrationRequest.Username))
        {
            response.Result = new BadRequestObjectResult("Username is already taken");
            response.Success = false;
            return response;
        }

        if (await IsEmailRegistered(registrationRequest.Email))
        {
            response.Result = new BadRequestObjectResult("Username is already taken");
            response.Success = false;
            return response;
        }

        CreatePasswordHash(registrationRequest.Password,
            out byte[] passwordHash,
            out byte[] passwordSalt);

        var user = new User()
        {
            Email = registrationRequest.Email,
            Username = registrationRequest.Username,
            FirstName = registrationRequest.FirstName,
            LastName = registrationRequest.LastName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _context.Users.Add(user);
        await _context.CompleteAsync();

        response.Data = user;
        response.Result = new OkObjectResult(user);
        
        return response;
}

    public async Task<ServiceResponse<string>> Login(UserDTO loginRequest, HttpResponse httpResponse)
    {
        var response = new ServiceResponse<string>();

        if (!(await IsEmailRegistered(loginRequest.Email)))
        {
            response.Result = new BadRequestObjectResult("User does not exist");
            response.Success = false;
            return response;
        }
        
        var user = _context.Users.Find(u => u.Email == loginRequest.Email).FirstOrDefault();
            
        if (!VerifyPasswordHash(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
        {
            response.Result = new BadRequestObjectResult("Incorrect Password");
            response.Success = false;
            return response;
        }
        
        string token = CreateToken(user);
        
        var refreshToken = GenerateRefreshToken();
        SetRefreshToken(refreshToken, user.Id, httpResponse);

        await _context.CompleteAsync();

        response.Data = token;
        response.Result = new OkObjectResult(token);
        
        return response;
    }

    public async Task<ServiceResponse<RefreshTokenDTO>> RefreshToken(HttpRequest httpRequest, HttpResponse httpResponse)
    {
        var response = new ServiceResponse<RefreshTokenDTO>();
        var refreshToken = httpRequest.Cookies["refreshToken"];
        var userId = httpRequest.Cookies["userId"];

        if (refreshToken == null)
        {
            response.Result = new UnauthorizedObjectResult("Refresh Token missing");
            response.Success = false;
            return response;
        }

        User user = await _context.Users.GetByIdAsync(int.Parse(userId!));

        if (!user.RefreshToken!.Equals(refreshToken))
        {
            response.Result = new UnauthorizedObjectResult("Invalid Refresh Token");
            response.Success = false;
            return response;
        }
            
        if(user.RefreshTokenExpires < DateTime.Now)
        {
            response.Result = new UnauthorizedObjectResult("Token expired");
            response.Success = false;
            return response;
        }
        
        string token = CreateToken(user);
        var newRefreshToken = GenerateRefreshToken();
        SetRefreshToken(newRefreshToken, user.Id, httpResponse);
        var refresh = new RefreshTokenDTO()
        {
            JWT = token,
            Token = newRefreshToken.Token,
            Expries = newRefreshToken.Expries
        };

        await _context.CompleteAsync();
        response.Data = refresh;
        response.Result = new OkObjectResult(refresh);
        
        return response;
    }

    public async Task<ServiceResponse<User>> GetUserById(int id)
    {
        var user = await _context.Users.GetByIdAsync(id);
        var response = new ServiceResponse<User>();

        if (user == null)
        {
            response.Success = false;
            response.Result = new NotFoundResult();
            return response;
        }

        response.Data = user;
        response.Result = new OkObjectResult(user);
        return response;
    }

    // Returns true if registered
    private async Task<bool> IsEmailRegistered(string email)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email)) return true;
        return false;
    }

    private async Task<bool> IsUsernameRegistered(string username)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username)) return true;
        return false;
    }
    
    private async Task<HttpResponse> SetRefreshToken(RefreshToken refreshToken, int userId, HttpResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            IsEssential = true,
            Secure = false,
            HttpOnly = false,
            Expires = refreshToken.Expries
        };
        
        response.Cookies.Append("refreshToken", refreshToken.Token,cookieOptions);
        response.Cookies.Append("userId", userId.ToString(), cookieOptions);

        var user = await _context.Users.GetByIdAsync(userId);
        user.RefreshToken = refreshToken.Token;
        user.RefreshTokenCreated = refreshToken.Created;
        user.RefreshTokenExpires = refreshToken.Expries;

        await _context.Users.Update(user);

        return response;
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