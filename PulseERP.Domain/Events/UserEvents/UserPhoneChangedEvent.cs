using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Domain.Events.UserEvents;

public sealed class UserPhoneChangedEvent(Guid userId, string newPhone) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string NewPhone { get; } = newPhone ?? throw new ArgumentNullException(nameof(newPhone));
}