namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record CreateCustomerRequest(
    string Name,
    string Phone,
    string Address,
    string CitizenId);
