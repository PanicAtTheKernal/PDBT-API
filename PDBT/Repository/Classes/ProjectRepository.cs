using PDBT.Data;
using PDBT.Models;

namespace PDBT.Repository;

public class ProjectRepository: GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(PdbtContext context):base(context)
    {
        
    }
}