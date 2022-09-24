using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PDBT.Data;

namespace PDBT.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly PdbtContext _context;
    public GenericRepository(PdbtContext context)
    {
        _context = context;
    }

    public bool Any(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Any(expression);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().AnyAsync(expression);
    }

    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public void AddAsync(T entity)
    {
        _context.Set<T>().AddAsync(entity);
    }
    
    public void AddRange(IEnumerable<T> entities)
    {
        _context.Set<T>().AddRange(entities);
    }


    public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression);
    }

    public virtual IEnumerable<T> GetAll()
    {
        return _context.Set<T>().ToList();
    }

    public virtual async Task<bool> Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return true;
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    public virtual T GetById(int id)
    {
        return _context.Set<T>().Find(id)!;
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return (await _context.Set<T>().FindAsync(id))!;
    }
    
    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
    }
}