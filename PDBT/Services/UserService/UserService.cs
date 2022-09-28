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
    
    private async void SetRefreshToken(RefreshToken refreshToken, int userId)
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