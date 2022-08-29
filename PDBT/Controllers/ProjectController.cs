using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDBT.Data;
using PDBT.Models;
using PDBT.Repository;

namespace PDBT.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IUnitOfWork _context;

        public ProjectController(IUnitOfWork context)
        {
            _context = context;
        }

        // GET: api/Project
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var enumerable = await _context.Projects.GetAllAsync();

            var projects = enumerable.ToList();
            if (!projects.Any())
            {
                return NotFound();
            }

            return projects;
        }

        // GET: api/Project/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }
            
            return project;
        }

        // PUT: api/Project/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectDTO projectDto)
        {
            if (id != projectDto.Id)
            {
                return BadRequest();
            }

            var issue = DtoToProject(projectDto);
            
            _context.Projects.Update(issue);
            
            
            //TODO Implement Project specific labels
            /*if (projectDto.Labels != null)
            {
                // Need to retrive list of current labels to prevent duplicate entries
                issue = await _context.Issues.GetByIdAsync(id);

                await InsertLabels(issue, issueDto.Labels);
            }*/

            try
            {
                await _context.CompleteAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Project
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(ProjectDTO projectDto)
        {
            if (!(await _context.Issues.GetAllAsync()).Any())
            {
                return Problem("Entity set 'PdbtContext.Projects'  is null.");
            }

            var project = DtoToProject(projectDto);
          
            _context.Projects.Add(project);

            //TODO Implement project specific labels
            /*if (projectDto.Labels != null)
            {
                //Prevents a null reference exception when adding the labels
                project.Labels = new List<Label>();
            
                project = await InsertLabels(project, projectDto.Labels);
            }*/

            
            await _context.CompleteAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, project);
        }

        // DELETE: api/Project/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            if (!(await _context.Projects.GetAllAsync()).Any())
            {
                return NotFound();
            }

            var project = await _context.Projects.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.CompleteAsync();

            return NoContent();
        }

        private Project DtoToProject(ProjectDTO projectDto) =>
            new Project()
            {
                Id = projectDto.Id,
                Name = projectDto.Name,
                Description = projectDto.Description
            };
        
        private bool ProjectExists(int id)
        {
            return _context.Projects.GetAll().Any(e => e.Id == id);
        }
    }
}
