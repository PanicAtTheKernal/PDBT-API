using PDBT.Data;
using PDBT.Models;

namespace PDBT.Repository;

public class IssueRepository : GenericRepository<Issue>, IIssueRepository
{
    public IssueRepository(PdbtContext context):base(context)
    {
        
    }
    
    public IEnumerable<Label> RetreiveLabels(int id)
    {
        throw new NotImplementedException();
    }
}