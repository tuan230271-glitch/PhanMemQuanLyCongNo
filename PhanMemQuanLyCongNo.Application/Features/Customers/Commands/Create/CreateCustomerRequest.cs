namespace PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Create;

public sealed record CreateCustomerRequest(
    string Name,
    string Phone,
    string Address,
    string CitizenId
);
