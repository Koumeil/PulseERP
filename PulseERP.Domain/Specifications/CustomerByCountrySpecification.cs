using System.Linq.Expressions;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Specifications;

public class CustomerByCountrySpecification : ISpecification<Customer>
{
    private readonly string _country;

    public CustomerByCountrySpecification(string country) => _country = country;

    public Expression<Func<Customer, bool>> ToExpression()
    {
        return c => c.Address.Country == _country;
    }
}
