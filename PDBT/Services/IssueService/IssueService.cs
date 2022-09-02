using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            response.Result = new NotFoundResult();
            return response;
        }

        response.Data = issues;
        response.Result = new OkObjectResult(response.Data);
        
        return response;
    }
    
    public async Task<ServiceResponse<Issue>> GetIssueById(int id, int projectId)
    {
        var issue = await _context.Issues.GetByIdAsync(id);
        var response = new ServiceResponse<Issue>();

        if (issue == null || issue.RootProjectID != projectId)
        {
            response.Success = false;
            response.Result = new NotFoundResult();
            return response;
        }

        response.Data = issue;
        response.Result = new OkObjectResult(issue);
        return response;
    }

    public async Task<ServiceResponse<Issue>> ConvertDto(int id, IssueDTO issueDto,int projectId)
    {
        var response = new ServiceResponse<Issue>();
        
        // Prevents the Id being updated
        if (id != issueDto.Id)
        {
            response.Success = false;
            response.Result = new BadRequestObjectResult("Invalid Id");
            return response;
        }
            
        // Prevent the root project Id from being updated
        if (projectId != issueDto.RootProjectID) 
        {
            response.Success = false;
            response.Result = new BadRequestObjectResult("Invalid Project Id");
            return response;
        }
        
        var issue = DtoToIssue(issueDto);

        // Retrieve the fully updated issue as the dto is only partially updated
        response.Data = await _context.Issues.GetByIdAsync(issue.Id);
        response.Result = new OkObjectResult(response.Data);

        return response;
    }

    public async Task<ServiceResponse<Issue>> UpdateIssue(Issue issue)
    {
        var response = new ServiceResponse<Issue>();
        await _context.Issues.Update(issue);

        response.Data = await _context.Issues.GetByIdAsync(issue.Id);
        response.Result = new OkObjectResult(response.Data);

        return response;
    }

    public async Task<ServiceResponse<Issue>> SaveChanges(int id)
    {
        try
        {
            await _context.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!IssueExists(id))
            {
                return new ServiceResponse<Issue>
                {
                    Result = new NotFoundResult(),
                    Success = false
                };
            }
            throw;
        }

        return new ServiceResponse<Issue>
        {
            Result = new NoContentResult(),
            Success = false
        };
    }

    private Issue DtoToIssue(IssueDTO issueDto) =>
        new()
        {
            Id = issueDto.Id,
            IssueName = issueDto.IssueName,
            Description = issueDto.Description,
            Type = issueDto.Type,
            Priority = issueDto.Priority,
            DueDate = issueDto.DueDate,
            TimeForCompletion = issueDto.TimeForCompletion,
            RootProjectID = issueDto.RootProjectID
        };
    
    private bool IssueExists(int id)
    {
        return _context.Issues.GetAll().Any(e => e.Id == id);
    }
}