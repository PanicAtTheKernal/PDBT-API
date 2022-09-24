using Microsoft.AspNetCore.Mvc;
using PDBT.Models;

namespace PDBT.Services.ProjectService;

public interface IProjectService
{
    Task<bool> ProjectExist(int projectId);
    Task<bool> UserBelongInProject(int projectId);
    Task<bool> UserBelongInProject(Project projectId);
    Task<ServiceResponse<Project>> GetProjectById(int projectId);
    Task<ServiceResponse<ActionResult>> ValidateUserAndProjectId(int projectId);
    Task<ServiceResponse<Issue>> InsertIssueIntoProject(Issue issue, int projectId);

}