using System.Linq.Expressions;

namespace DAL.Interfaces;

public interface ISpecifications<TEntity> where TEntity : class
{
    ICollection<Expression<Func<TEntity, object>>> IncludeExpressions { get; }
    Expression<Func<TEntity, bool>>? Criteria { get; }
    Expression<Func<TEntity, object>>? OrderBy { get; }
    Expression<Func<TEntity, object>>? OrderByDescending { get; }
    bool IsPaginated { get; }
    int Take { get; }
    int Skip { get; }
    void DisablePagination();
}
