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
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success) return response.Data!;

            var issuesResponse = await _issueService.GetAllIssue(projectId);

            return issuesResponse.Result;
        }

        // GET: api/Issue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Issue>> GetIssue(int id, int projectId)
        {
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
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success) return response.Data!;

            var issueResponse = await _issueService.ConvertDto(id, issueDto, projectId);
            if (!issueResponse.Success) return issueResponse.Result;

            if (issueDto.Labels != null)
            {
                issueResponse = await _labelService.UpdateLabelsInIssue(issueResponse.Data, issueDto.Labels);
            } 
            
            await _issueService.UpdateIssue(issueResponse.Data);
            issueResponse = await _issueService.SaveChanges(id);
            return issueResponse.Result;
        }

        // POST: api/Issue
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Issue>> PostIssue(IssueDTO issueDto, int projectId)
        {
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success)
            {
                return response.Data!;
            }
            
            if (!(await _context.Issues.GetAllAsync()).Any())
            {
              return Problem("Entity set 'PdbtContext.Issues'  is null.");
            }

            // var issue = DtoToIssue(issueDto);

        //     _context.Issues.Add(issue);
        //
        //     if (issueDto.Labels != null)
        //     {
        //       //Prevents a null reference exception when adding the labels
        //       issue.Labels = new List<Label>();
        //
        //       // issue = await InsertLabels(issue, issueDto.Labels);
        //     }
        //
        //     issue = await InsertIssue(issue, projectId);
        //
        //     await _context.CompleteAsync();
        //
            // return CreatedAtAction("GetIssue", new { id = issue.Id, projectId }, issue);
            return Ok();
        }

        // DELETE: api/Issue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id, int projectId)
        {
            var response = await _projectService.ValidateUserAndProjectId(projectId);
            if (!response.Success)
            {
                return response.Data!;
            }
            
            if (!(await _context.Issues.GetAllAsync()).Any())
            {
                return NotFound();
            }

            var issue = await _context.Issues.GetByIdAsync(id);
            if (issue == null)
            {
                return NotFound();
            }

            _context.Issues.Remove(issue);
            await _context.CompleteAsync();

            return NoContent();
        }

        private async Task<Project?> RetrieveProject(int id)
        {
            var project = await _context.Projects.GetByIdAsync(id);

            if (project == null)
                return null;
            
            return project;
        }

        private async Task<Issue?> InsertIssue(Issue issue,int projectId)
        {
            var rootProject = await RetrieveProject(projectId);
            issue.RootProjectID = projectId;
            issue.RootProject = rootProject!;
            rootProject!.Issues.Add(issue);
            
            return issue;
        }
    }
}
