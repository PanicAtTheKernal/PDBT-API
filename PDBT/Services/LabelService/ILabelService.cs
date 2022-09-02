using PDBT.Models;

namespace PDBT.Services.LabelService;

public interface ILabelService
{
    Task<ServiceResponse<IEnumerable<Label>>> GetAllLabel(int projectId);
    Task<ServiceResponse<Label>> GetLabelById(int id, int projectId);
    Task<ServiceResponse<Label>> UpdateLabel(int id, LabelDTO labelDto,int projectId);
    Task<ServiceResponse<Issue>> UpdateLabelsInIssue(Issue issue, ICollection<LabelDTO> labelsToInsert);
}