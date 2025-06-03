namespace PulseERP.Infrastructure.Seeds;

public class ProductSeed
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Brand { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool IsService { get; set; }
}

public class CustomerSeed
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string CompanyName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? Industry { get; set; }
    public string? Source { get; set; }
    public bool IsVIP { get; set; }
    public bool IsActive { get; set; }
    public DateTime FirstContactDate { get; set; }
    public DateTime? LastInteractionDate { get; set; }
}

public class UserSeed
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class RootSeed
{
    public List<ProductSeed> Products { get; set; } = new();
    public List<CustomerSeed> Customers { get; set; } = new();
    public List<UserSeed> Users { get; set; } = new();
}
