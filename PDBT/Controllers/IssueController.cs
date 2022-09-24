using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDBT.Data;
using PDBT.Models;
using PDBT.Repository;
using PDBT.Services.LabelService;
using PDBT.Services.ProjectService;

namespace PDBT.Controllers
{
    [Route("api/{projectId}/[controller]")]
    [ApiController, Authorize]
    public class IssueController : ControllerBase
    {
        private readonly IUnitOfWork _context;
        private readonly IIssueService _issueService;
        private readonly IProjectService _projectService;
        private readonly ILabelService _labelService;

        public IssueController(IUnitOfWork unitOfWork, IIssueService issueService, IProjectService projectService,
            ILabelService labelService)
        {
            _context = unitOfWork;
            _issueService = issueService;
            _projectService = projectService;
            _labelService = labelService;
        }

        // GET: api/Issue
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Issue>>> GetIssues(int projectId)
        {
            // Returns 404 or 400 depending on the problem
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success) return response.Data!;

            var issuesResponse = await _issueService.GetAllIssue(projectId);

            return issuesResponse.Result;
        }

        // GET: api/Issue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Issue>> GetIssue(int id, int projectId)
        {
            // Returns 404 or 400 depending on the problem
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success) return response.Data!;
            
            var issueResponse = await _issueService.GetIssueById(id, projectId);

            return issueResponse.Result;
        }

        // PUT: api/Issue/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<Issue>> PutIssue(int id, IssueDTO issueDto, int projectId)
        {
            // Returns 404 or 400 depending on the problem
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success) return response.Data!;

            var issueResponse = await _issueService.ConvertDto(id, issueDto, projectId);
            if (!issueResponse.Success) return issueResponse.Result;

            if (issueDto.Labels != null)
                issueResponse = await _labelService.UpdateLabelsInIssue(issueResponse.Data!, issueDto.Labels);

            await _issueService.UpdateIssue(issueResponse.Data!);
            issueResponse = await _issueService.SaveChanges(id);
            return issueResponse.Result;
        }

        // POST: api/Issue
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Issue>> PostIssue(IssueDTO issueDto, int projectId)
        {
            // Returns 404 or 400 depending on the problem
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success) return response.Data!;

            var issueResponse = await _issueService.ConvertDto(issueDto.Id, issueDto, projectId);
            if (!issueResponse.Success) return issueResponse.Result;

            issueResponse = await _issueService.AddIssue(issueResponse.Data!);

            if (issueDto.Labels != null)
                issueResponse = await _labelService.UpdateLabelsInIssue(issueResponse.Data!, issueDto.Labels);

            issueResponse = await _projectService.InsertIssueIntoProject(issueResponse.Data!, projectId);
            await _issueService.SaveChanges(issueResponse.Data!.Id);
            
            return issueResponse.Data;
        }

        // DELETE: api/Issue/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Issue>> DeleteIssue(int id, int projectId)
        {
            // Returns 404 or 400 depending on the problem
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success) return response.Data!;

            var results = await _issueService.DeleteIssue(id, projectId);
            return results.Result;
        }
    }
}
