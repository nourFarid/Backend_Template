using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public static class SpecificationsEvaluator
{
    public static IQueryable<TEntity> BuildQuery<TEntity>(IQueryable<TEntity> query, ISpecifications<TEntity>? specifications)
        where TEntity : class
    {
        if (specifications is null)
            return query;

        if (specifications.Criteria is not null)
        {
            query = query.Where(specifications.Criteria);
        }

        if (specifications.IncludeExpressions is not null && specifications.IncludeExpressions.Any())
        {
            foreach (var includeExpression in specifications.IncludeExpressions)
            {
                query = query.Include(includeExpression);
            }
        }

        if (specifications.OrderBy is not null)
        {
            query = query.OrderBy(specifications.OrderBy);
        }
        else if (specifications.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specifications.OrderByDescending);
        }

        if (specifications.IsPaginated)
        {
            query = query.Skip(specifications.Skip).Take(specifications.Take);
        }

        return query;
    }
}
