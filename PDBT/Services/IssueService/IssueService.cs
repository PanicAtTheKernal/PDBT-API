using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Services.IssueService;

public class IssueService : IIssueService
{
    private readonly IUnitOfWork _context;

    public IssueService(IUnitOfWork context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<IEnumerable<Issue>>> GetAllIssue(int projectId)
    {
        var enumerable = await _context.Issues.GetAllAsync();

        var issues = enumerable.Where(i => i.RootProjectID == projectId).ToList();
        var response = new ServiceResponse<IEnumerable<Issue>>();

        if (!issues.Any())
        {
            response.Success = false;
            response.Message = "NotFound";
            return response;
        }

        response.Data = issues;

        return response;
    }
}