namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record UpdateCustomerRequest(
    string Name,
    string Phone,
    string Address,
    string CitizenId
);