using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDBT.Data;
using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Controllers
{
    [Route("api/{projectId}/[controller]")]
    [ApiController]
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
            if (!ProjectExists(projectId))
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
            if (!ProjectExists(projectId))
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
        public async Task<IActionResult> PutIssue(int id, IssueDTO issueDto)
        {
            if (id != issueDto.Id)
            {
                return BadRequest();
            }

            var issue = DtoToIssue(issueDto);
            
            _context.Issues.Update(issue);
            
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
        public async Task<ActionResult<Issue>> PostIssue(IssueDTO issueDto)
        {
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



          await _context.CompleteAsync();

          return CreatedAtAction("GetIssue", new { id = issue.Id }, issue);
        }

        // DELETE: api/Issue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id)
        {
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
            new Issue()
            {
                Id = issueDto.Id,
                IssueName = issueDto.IssueName,
                Description = issueDto.Description,
                Type = issueDto.Type,
                Priority = issueDto.Priority,
                DueDate = issueDto.DueDate,
                TimeForCompletion = issueDto.TimeForCompletion
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
        
        private bool ProjectExists(int id)
        {
            return _context.Projects.GetAll().Any(e => e.Id == id);
        }
        
        private bool IssueExists(int id)
        {
            return _context.Issues.GetAll().Any(e => e.Id == id);
        }
        
    }
}
