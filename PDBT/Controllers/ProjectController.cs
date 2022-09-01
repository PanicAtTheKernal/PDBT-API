using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var projectsWithUser = new List<Project>();

            foreach (var project in projects)
            {
                if (await verifyUser(project))
                {
                    projectsWithUser.Add(project);
                }
            }
            
            if (!projects.Any())
            {
                return NotFound();
            }

            return projectsWithUser;
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

            if (!await verifyUser(project))
            {
                return Forbid();
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

            var project = DtoToProject(projectDto);
            
            _context.Projects.Update(project);
            project = await _context.Projects.GetByIdAsync(project.Id);

            //TODO Implement Project specific labels

            try
            {
                if (await verifyUser(project))
                {
                    await _context.CompleteAsync();
                }
                else
                {
                    return Forbid();
                }
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
            if (User?.Identity?.Name != null)
            {
                var authUserId = Int32.Parse(User.Identity.Name);
                var authUser = await _context.Users.GetByIdAsync(authUserId);

                // A new project will always have the user section as null so we don't need check if it is null
                project.Users = new List<User>();
                
                project.Users.Add(authUser);
            }


            _context.Projects.Add(project);

            //TODO Implement project specific labels

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

            if (await verifyUser(project))
            {
                _context.Projects.Remove(project);
                await _context.CompleteAsync();
                
                return NoContent();
            }

            return Forbid();
        }

        private Project DtoToProject(ProjectDTO projectDto) =>
            new Project()
            {
                Id = projectDto.Id,
                Name = projectDto.Name,
                Description = projectDto.Description
            };

        private async Task<bool> verifyUser(Project project)
        {
            if (User?.Identity?.Name != null)
            {
                var authUserId = Int32.Parse(User.Identity.Name);
                var authUser = await _context.Users.GetByIdAsync(authUserId);

                if (project.Users.Contains(authUser))
                {
                    return true;
                }
            }
            return false;
        }
        
        private bool ProjectExists(int id)
        {
            return _context.Projects.GetAll().Any(e => e.Id == id);
        }
    }
}
