using PDBT.Models;

namespace PDBT.Services.IssueService;

public interface IIssueService
{ 
    Task<ServiceResponse<IEnumerable<Issue>>> GetAllIssue(int projectId);
}