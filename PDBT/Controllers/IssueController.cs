using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDBT.Data;
using PDBT.Models;

namespace PDBT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController : ControllerBase
    {
        private readonly PdbtContext _context;

        public IssueController(PdbtContext context)
        {
            _context = context;
        }

        // GET: api/Issue
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Issue>>> GetIssues()
        {
            var issues = await _context.Issues
                .Include(issue => issue.Labels)
                .ToListAsync();
            
            if (issues.Count == 0)
            {
                return NotFound();
            }

            return issues;
        }

        // GET: api/Issue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Issue>> GetIssue(int id)
        {
            var issue = await _context.Issues.Where(i => i.Id == id)
                .Include(i => i.Labels)
                .FirstOrDefaultAsync();

            if (issue == null)
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
            
            _context.Entry(issue).State = EntityState.Modified;
            
            if (issueDto.Labels != null)
            {
                // Need to retrive list of current labels to prevent duplicate entries
                issue = await _context.Issues.Where(i => i.Id == id)
                    .Include(i => i.Labels)
                    .FirstOrDefaultAsync();

                if (issue != null) issue = await InsertLabels(issue, issueDto.Labels);
            }

            try
            {
                await _context.SaveChangesAsync();
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
        public async Task<ActionResult<IssueDTO>> PostIssue(IssueDTO issueDto)
        {
          if (_context.Issues == null)
          {
              return Problem("Entity set 'PdbtContext.Issues'  is null.");
          }

          var issue = DtoToIssue(issueDto);
          
          _context.Issues.Add(issue);
          // await _context.SaveChangesAsync();

          if (issueDto.Labels != null)
          {
              //Prevents a null reference exception when adding the labels
              issue.Labels = new List<Label>();

              issue = await InsertLabels(issue, issueDto.Labels);
          }



          await _context.SaveChangesAsync();

          return CreatedAtAction("GetIssue", new { id = issue.Id }, issue);
        }

        // DELETE: api/Issue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id)
        {
            if (_context.Issues == null)
            {
                return NotFound();
            }
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
            {
                return NotFound();
            }

            // var lb = await _context.LabelDetails.Where(e => e.IssueId == id).ToListAsync();
            // _context.LabelDetails.RemoveRange(lb);

            
            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IssueDTO IssueToDto(Issue issue) =>
            new IssueDTO()
            {
                Id = issue.Id,
                IssueName = issue.IssueName,
                Description = issue.Description,
                Type = issue.Type,
                Priority = issue.Priority,
                DueDate = issue.DueDate,
                TimeForCompletion = issue.TimeForCompletion,
            };

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

        private LabelDTO LabelToDto(Label label) =>
            new LabelDTO()
            {
                Id = label.Id,
                Name = label.Name
            };

        private async Task<Label?> RetrieveLabel(int id)
        {
            var label = await _context.Labels.FindAsync(id);

            if (label == null)
                return null;
            
            return label;
        }

        private async Task<Issue?> InsertLabels(Issue issue, ICollection<LabelDTO> labelsDtos)
        {
            foreach (var labelDto in labelsDtos)
            {
                var label = await RetrieveLabel(labelDto.Id);

                if (label == null)
                {
                    return null;
                }

                if (issue.Labels.All(l => l.Id != label.Id))
                    issue.Labels.Add(label);
                    
            }

            return issue;
        }
        
        private bool IssueExists(int id)
        {
            return (_context.Issues?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        
    }
}
