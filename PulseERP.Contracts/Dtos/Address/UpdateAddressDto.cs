namespace PulseERP.Contracts.Dtos.Address;

// DTO pour mettre à jour une adresse partielle (ex: PATCH)
public record UpdateAddressDto(
    string? Street = null,
    string? City = null,
    string? ZipCode = null,
    string? Country = null
);
