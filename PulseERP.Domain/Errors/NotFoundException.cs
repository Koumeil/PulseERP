namespace PulseERP.Domain.Errors;

public class NotFoundException(string name, object key) :
    Exception($"Entity \"{name}\" ({key}) was not found.");