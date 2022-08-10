using PDBT.Data;
using PDBT.Models;
using PDBT.Repository.Classes;

namespace PDBT.Repository;

public class LabelRepository: GenericRepository<Label>, ILabelRepository
{
    public LabelRepository(PdbtContext context):base(context)
    {
    }
}