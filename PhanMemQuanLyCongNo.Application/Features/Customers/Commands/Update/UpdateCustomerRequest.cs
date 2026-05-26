namespace PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Update;

public sealed record UpdateCustomerRequest(
    string Name,
    string Phone,
    string Address,
    string CitizenId
);