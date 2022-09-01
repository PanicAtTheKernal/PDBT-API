using Microsoft.AspNetCore.Mvc;
using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Services.ProjectService;

public class ProjectService: IProjectService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _context;

    public ProjectService(IHttpContextAccessor httpContextAccessor, IUnitOfWork context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }
    
    public async Task<bool> ProjectExist(int projectId)
    {
        return (await _context.Projects.GetAllAsync()).Any(e => e.Id == projectId);
    }

    public async Task<bool> UserBelongInProject(int projectId)
    {
        var project = await _context.Projects.GetByIdAsync(projectId);
        return await UserBelongInProject(project);
    }

    public async Task<bool> UserBelongInProject(Project project)
    {
        var userId = _httpContextAccessor.HttpContext?.User.Identity?.Name;    
        
        if (userId != null)
        {
            var authUserId = Int32.Parse(userId);
            var authUser = await _context.Users.GetByIdAsync(authUserId);

            if (project.Users.Contains(authUser))
                return true;
        }
        return false;
    }

    public async Task<ServiceResponse<ActionResult>> ValidateUserAndProjectId(int projectId)
    {
        var respone = new ServiceResponse<ActionResult>();
        
        if (!await ProjectExist(projectId))
        {
            respone.Success = false;
            respone.Data = new NotFoundObjectResult("Project Does not exist");
            return respone;
        }

        if (!await UserBelongInProject(projectId))
        {
            respone.Success = false;
            respone.Data = new ForbidResult();
            return respone;
        }
        
        return respone;
    }
}