using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDBT.Data;
using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Controllers
{
    [Route("api/{projectId}/[controller]")]
    [ApiController, Authorize]
    public class IssueController : ControllerBase
    {
        private readonly IUnitOfWork _context;

        public IssueController(IUnitOfWork unitOfWork)
        {
            _context = unitOfWork;
        }

        // GET: api/Issue
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Issue>>> GetIssues(int projectId)
        {
            if (! await ProjectExists(projectId))
            {
                return NotFound("Project not found");
            }
            
            var enumerable = await _context.Issues.GetAllAsync();

            var issues = enumerable.Where(i => i.RootProjectID == projectId).ToList();
            if (!issues.Any())
            {
                return NotFound();
            }

            return issues;
        }

        // GET: api/Issue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Issue>> GetIssue(int id, int projectId)
        {
            if (! await ProjectExists(projectId))
            {
                return NotFound("Project not found");
            }
            
            var issue = await _context.Issues.GetByIdAsync(id);

            if (issue == null || issue.RootProjectID != projectId)
            {
                return NotFound();
            }
            
            return issue;
        }

        // PUT: api/Issue/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssue(int id, IssueDTO issueDto, int projectId)
        {
            
            if (id != issueDto.Id)
            {
                return BadRequest();
            }

            var issue = DtoToIssue(issueDto);

            await _context.Issues.Update(issue);

            if (! await ProjectExists(projectId))
            {
                return NotFound("Project not found");
            }
            
            if (issue.RootProjectID != projectId)
            {
                return BadRequest("Invaild ProjectId");
            }
            
            if (issueDto.Labels != null)
            {
                // Need to retrive list of current labels to prevent duplicate entries
                issue = await _context.Issues.GetByIdAsync(id);

                await InsertLabels(issue, issueDto.Labels);
            }

            try
            {
                await _context.CompleteAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IssueExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Issue
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Issue>> PostIssue(IssueDTO issueDto, int projectId)
        {
            if (! await ProjectExists(projectId))
            {
                return NotFound("Project not found");
            }

            if (!(await _context.Issues.GetAllAsync()).Any())
            {
              return Problem("Entity set 'PdbtContext.Issues'  is null.");
            }

            var issue = DtoToIssue(issueDto);

            _context.Issues.Add(issue);

            if (issueDto.Labels != null)
            {
              //Prevents a null reference exception when adding the labels
              issue.Labels = new List<Label>();

              issue = await InsertLabels(issue, issueDto.Labels);
            }

            issue = await InsertIssue(issue, projectId);

            await _context.CompleteAsync();

            return CreatedAtAction("GetIssue", new { id = issue.Id, projectId }, issue);
        }

        // DELETE: api/Issue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id, int projectId)
        {
            if (! await ProjectExists(projectId))
            {
                return NotFound("Project not found");
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
        
        private async Task<Label?> RetrieveLabel(int id)
        {
            var label = await _context.Labels.GetByIdAsync(id);

            if (label == null)
                return null;
            
            return label;
        }

        private async Task<Issue?> InsertLabels(Issue issue, ICollection<LabelDTO> labelsDtos)
        {
            foreach (var labelDto in labelsDtos)
            {
                var label = await RetrieveLabel(labelDto.Id);

                if (label != null)
                {
                    if (issue.Labels.All(l => l.Id != label.Id))
                        issue.Labels.Add(label);
                }

                    
            }

            return issue;
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
        
        private async Task<bool> ProjectExists(int id)
        {
            return (await _context.Projects.GetAllAsync()).Any(e => e.Id == id);
        }
        
        private bool IssueExists(int id)
        {
            return _context.Issues.GetAll().Any(e => e.Id == id);
        }
        
    }
}
