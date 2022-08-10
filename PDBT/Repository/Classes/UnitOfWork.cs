using PDBT.Data;
using PDBT.Repository.Classes;

namespace PDBT.Repository;

public class UnitOfWork: IUnitOfWork
{
    private readonly PdbtContext _context;
    public IIssueRepository Issues { get; set; }
    public ILabelRepository Labels { get; set; }
    public IUserRepository Users { get; set; }
    public IProjectRepository Projects { get; set; }


    public UnitOfWork(PdbtContext context)
    {
        _context = context;
        Issues = new IssueRepository(context);
        Labels = new LabelRepository(context);
        Users = new UserRepository(context);
        Projects = new ProjectRepository(context);
    }

    public int Complete()
    {
        return _context.SaveChanges();
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }
    public void Dispose()
    {
        _context.Dispose();
    }
}