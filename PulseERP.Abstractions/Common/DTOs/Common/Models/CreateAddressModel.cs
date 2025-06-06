// CreateAddressModel.cs
namespace PulseERP.Abstractions.Common.DTOs.Common.Models;

public sealed record CreateAddressModel(string Street, string City, string ZipCode, string Country);
