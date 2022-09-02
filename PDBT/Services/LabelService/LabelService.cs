using Microsoft.AspNetCore.Mvc;
using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Services.LabelService;

public class LabelService : ILabelService
{
    private readonly IUnitOfWork _context;

    public LabelService(IUnitOfWork context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<IEnumerable<Label>>> GetAllLabel(int projectId)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResponse<Label>> GetLabelById(int id, int projectId)
    {
        var label = await _context.Labels.GetByIdAsync(id);
        var response = new ServiceResponse<Label>();

        if (label == null || label.RootProject.Id != projectId)
        {
            response.Success = false;
            response.Result = new NotFoundResult();
            return response;
        }

        response.Data = label;
        response.Result = new OkObjectResult(label);
        return response;
    }

    public async Task<ServiceResponse<Label>> UpdateLabel(int id, LabelDTO labelDto, int projectId)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResponse<Issue>> UpdateLabelsInIssue(Issue issue, ICollection<LabelDTO> labelsToInsert)
    {
        foreach (var labelDto in labelsToInsert)
        {
            var label = await GetLabelById(labelDto.Id, issue.RootProjectID);

            if (label.Success)
            {
                if (issue.Labels.All(l => l.Id != label.Data.Id))
                    issue.Labels.Add(label.Data);
            }
        }

        return new ServiceResponse<Issue>()
        {
            Data = issue,
            Result = new OkObjectResult(issue),
            Success = true
        };
    }
}