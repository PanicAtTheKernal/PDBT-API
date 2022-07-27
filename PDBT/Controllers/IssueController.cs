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
        public async Task<IActionResult> PutIssue(int id, IssueDTO issueDto)
        {
            if (id != issueDto.Id)
            {
                return BadRequest();
            }

            var issue = DtoToIssue(issueDto);
            
            _context.Entry(issue).State = EntityState.Modified;

            if (issueDto.Labels != null) InsertLabels(issueDto.Labels, issue.Id);

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
          
          //Need to save here otherwise the id need to create an entry for label detail will not be present as an forign
          //key
          await _context.SaveChangesAsync();
          
          if (issueDto.Labels != null) InsertLabels(issueDto.Labels, issue.Id);
          
          await _context.SaveChangesAsync();

          // This is done to prevent a json cycle exception from being thrown
          var returnIssue = IssueToDto(await _context.Issues.FindAsync(issue.Id));
          returnIssue.Labels = await RetrieveLabels(returnIssue.Id);
          
          return CreatedAtAction("GetIssue", new { id = issue.Id }, returnIssue);
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
        
        private async Task<ICollection<LabelDTO>> RetrieveLabels(int id)
        {
            //Retrieve a list of the labels associated with the issue
            var labelsDetails = await _context.LabelDetails.Where(ld => ld.IssueId.Equals(id))
                .ToListAsync();
            ICollection<LabelDTO> issueLabels = new List<LabelDTO>();
            
            foreach (var ld in labelsDetails)
            {
                var label = await _context.Labels.FindAsync(ld.LabelId);
                if (label != null)
                    issueLabels.Add(LabelToDto(label));
            }

            return issueLabels;
        }

        private void InsertLabels(ICollection<LabelDTO> labelds, int issueId)
        {
            foreach (LabelDTO lb in labelds)
            {
                LabelDetail tempLd = new LabelDetail();
                tempLd.IssueId = issueId;
                tempLd.LabelId = lb.Id;
                if (!LabelDetailExists(issueId, lb.Id))
                {
                    _context.LabelDetails.Add(tempLd);
                }
            }
            
        }
        
        private bool IssueExists(int id)
        {
            return (_context.Issues?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool LabelDetailExists(int issueId, int labelId)
        {
            return (_context.LabelDetails?.Any(e => e.IssueId == issueId && e.LabelId == labelId))
                .GetValueOrDefault();
        }
    }
}
