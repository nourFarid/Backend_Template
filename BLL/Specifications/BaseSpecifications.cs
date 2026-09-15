using DAL.Interfaces;
using System.Linq.Expressions;

namespace BLL.Specifications;

public abstract class BaseSpecifications<TEntity> : ISpecifications<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>>? Criteria { get; }
    public ICollection<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = [];
    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
    public bool IsPaginated { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }

    protected BaseSpecifications() { }

    protected BaseSpecifications(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        IncludeExpressions.Add(includeExpression);
    }

    protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    protected void ApplyPagination(int pageSize, int pageIndex)
    {
        IsPaginated = true;
        Skip = (pageIndex - 1) * pageSize;
        Take = pageSize;
    }

    public void DisablePagination() => IsPaginated = false;
}
