using Microsoft.AspNetCore.Mvc;
using PDBT.Models;

namespace PDBT.Services.UserService;

public interface IUserService
{
    Task<ServiceResponse<User>> Register(UserRegistration registrationRequest);
    Task<ServiceResponse<User>> Login(UserDTO loginRequest);

}