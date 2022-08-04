using PDBT.Data;
using PDBT.Models;

namespace PDBT.Repository;

public class LabelRepository: GenericRepository<Label>, ILabelRepository
{
    public LabelRepository(PdbtContext context):base(context)
    {
    }
}