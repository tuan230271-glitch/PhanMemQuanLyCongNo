using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record UpdateDebtStatusRequest(
    TrangThaiCongNo Status
);