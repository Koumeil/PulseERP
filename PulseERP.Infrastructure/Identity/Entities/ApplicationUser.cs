using Microsoft.AspNetCore.Identity;
using PulseERP.Domain.Entities;

namespace PulseERP.Infrastructure.Identity.Entities;

public class ApplicationUser : IdentityUser<Guid> 
{
    public Guid DomainUserId { get; private set; }
    public virtual User? DomainUser { get; private set; }

    public ApplicationUser(Guid domainUserId)
    {
        DomainUserId = domainUserId;
    }

    // Pour EF Core (constructeur vide requis)
    protected ApplicationUser() { }

    public void SetDomainUser(User domainUser)
    {
        DomainUser = domainUser ?? throw new ArgumentNullException(nameof(domainUser));
        DomainUserId = domainUser.Id;
    }
}
