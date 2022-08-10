using PDBT.Data;
using PDBT.Models;

namespace PDBT.Repository;

public class UserRepository: GenericRepository<User>, IUserRepository
{
    public UserRepository(PdbtContext context):base(context)
    {
        
    }
}