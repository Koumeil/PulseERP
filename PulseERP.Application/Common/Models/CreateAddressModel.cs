// CreateAddressModel.cs
namespace PulseERP.Application.Common.Models;

public sealed record CreateAddressModel(string Street, string City, string ZipCode, string Country);
