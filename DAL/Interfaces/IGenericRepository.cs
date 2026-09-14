using DAL.Interfaces;
using System.Linq.Expressions;

namespace DAL.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<List<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<int> GetCountAsync();
    Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ChangeStatusAsync(int id, string? userId, string propertyName = "IsActive");
    Task<List<T>> AddRangeAsync(List<T> entities);
    Task<bool> UpdateRangeAsync(List<T> entities);
    Task<IEnumerable<T>> GetAllAsync(ISpecifications<T> specs);
    Task<int> GetCountAsync(ISpecifications<T> specs);
}
