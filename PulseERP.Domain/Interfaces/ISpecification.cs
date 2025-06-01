using System.Linq.Expressions;

namespace PulseERP.Domain.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);
}
