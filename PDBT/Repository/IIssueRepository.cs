using PDBT.Models;

namespace PDBT.Repository;

public interface IIssueRepository : IGenericRepository<Issue>
{
    IEnumerable<Label> RetreiveLabels(int id);
}