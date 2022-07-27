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
        public async Task<ActionResult<IEnumerable<IssueDTO>>> GetIssues()
        {
            var issues = await _context.Issues.ToListAsync();
            
            if (issues == null)
            {
                return NotFound();
            }

            List<IssueDTO> issuesDto = new List<IssueDTO>();

            foreach (var issue in issues)
            {
                var issueDto = IssueToDto(issue);
                issueDto.Labels = await RetrieveLabels(issueDto.Id);
                issuesDto.Add(issueDto);
            }
            
            return issuesDto;
        }

        // GET: api/Issue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueDTO>> GetIssue(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            
            if (issue == null)
            {
                return NotFound();
            }

            //Convert to DTO so we can attach the list of labels to it
            var issueDto = IssueToDto(issue);

            issueDto.Labels = await RetrieveLabels(issueDto.Id);

            return issueDto;
        }

        // PUT: api/Issue/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssue(int id, Issue issue)
        {
            if (id != issue.Id)
            {
                return BadRequest();
            }

            _context.Entry(issue).State = EntityState.Modified;

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
        public async Task<ActionResult<Issue>> PostIssue(Issue issue)
        {
          if (_context.Issues == null)
          {
              return Problem("Entity set 'PdbtContext.Issues'  is null.");
          }
          
          _context.Issues.Add(issue);
          try
          {
              if (issue.Labels != null)
                  foreach (LabelDetail lb in issue.Labels)
                  {
                      _context.LabelDetails.Add(lb);
                  }

          }
          catch (Exception e)
          {
              Console.WriteLine(e);
              throw;
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
                TimeForCompletion = issue.TimeForCompletion
            };

        private async Task<ICollection<Label>> RetrieveLabels(int id)
        {
            //Retrieve a list of the labels associated with the issue
            var labelsDetails = await _context.LabelDetails.Where(ld => ld.IssueId.Equals(id))
                .ToListAsync();
            ICollection<Label> issueLabels = new List<Label>();
            
            foreach (var ld in labelsDetails)
            {
                var label = await _context.Labels.FindAsync(ld.LabelId);
                if (label != null)
                    issueLabels.Add(label);
            }

            return issueLabels;
        }

        private bool IssueExists(int id)
        {
            return (_context.Issues?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
