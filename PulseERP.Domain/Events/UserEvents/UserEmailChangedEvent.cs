using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Domain.Events.UserEvents;
public sealed class UserEmailChangedEvent(Guid userId, string newEmail) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string NewEmail { get; } = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
}
