using System.Collections.Concurrent;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Models;
using PhanMemQuanLyCongNo.Domain.Entities;
using PhanMemQuanLyCongNo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using PhanMemQuanLyCongNo.Infrastructure.Persistence.DbContext;

namespace PhanMemQuanLyCongNo.Infrastructure.Services;

public class DebtManagementService : IDebtManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private readonly List<Tenant> _tenants = [];
    private readonly List<AppUser> _users = [];
    private readonly List<KhachHang> _customers = [];
    private readonly List<Contract> _contracts = [];
    private readonly List<CongNo> _debts = [];
    private readonly List<ThanhToan> _payments = [];
    private readonly List<CollectionTask> _tasks = [];
    private readonly List<NotificationLog> _notifications = [];
    private readonly List<AuditLog> _auditLogs = [];
    private readonly ConcurrentDictionary<string, Guid> _tenantAliases = new(StringComparer.OrdinalIgnoreCase);

    public DebtManagementService(
        ApplicationDbContext context,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }
    public Guid DefaultTenantId =>
        _tenants.FirstOrDefault()?.Id ?? Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid ResolveTenant(string? tenantId)
    {
        if (Guid.TryParse(tenantId, out var parsed) && _tenants.Any(t => t.Id == parsed))
        {
            return parsed;
        }

        if (!string.IsNullOrWhiteSpace(tenantId) && _tenantAliases.TryGetValue(tenantId, out var alias))
        {
            return alias;
        }

        return DefaultTenantId;
    }

    public IReadOnlyCollection<Tenant> GetTenants() => _tenants;

    public IReadOnlyCollection<AppUser> GetUsers(Guid tenantId) =>
        _users
            .Where(u => u.TenantId == tenantId || u.Role == UserRole.SuperAdmin)
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FullName)
            .ToArray();

    public AppUser CreateUser(Guid tenantId, CreateUserRequest request)
    {
        ValidateUserRequest(tenantId, request.FullName, request.Email, request.Role);

        var user = new AppUser(Guid.NewGuid(), tenantId, request.FullName.Trim(), request.Email.Trim(), request.Role, request.IsActive);
        _users.Add(user);
        AddAudit(tenantId, "TenantAdmin", "Create", "User");
        return user;
    }

    public AppUser UpdateUser(Guid tenantId, Guid userId, UpdateUserRequest request)
    {
        var index = _users.FindIndex(u => u.Id == userId && u.TenantId == tenantId);
        if (index < 0)
        {
            throw new InvalidOperationException("Nguoi dung khong ton tai.");
        }

        ValidateUserRequest(tenantId, request.FullName, request.Email, request.Role, userId);

        var current = _users[index];
        var updated = new AppUser
        {
            Id = current.Id,
            TenantId = current.TenantId,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Role = request.Role,
            IsActive = request.IsActive,
            PasswordHash = current.PasswordHash
        };
        _users[index] = updated;
        AddAudit(tenantId, "TenantAdmin", "Update", "User");
        return updated;
    }

    public void DeleteUser(Guid tenantId, Guid userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId && u.TenantId == tenantId)
            ?? throw new InvalidOperationException("Nguoi dung khong ton tai.");

        if (_tasks.Any(t => t.TenantId == tenantId && t.AssignedTo == userId))
        {
            throw new InvalidOperationException("Nguoi dung da duoc gan cong viec, hay khoa tai khoan thay vi xoa.");
        }

        _users.Remove(user);
        AddAudit(tenantId, "TenantAdmin", "Delete", "User");
    }

    public object Login(LoginRequest request)
    {
        var user = _context.Users
            .FirstOrDefault(u =>
                u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)
                && u.IsActive);

        if (user is null)
        {
            throw new InvalidOperationException("Sai tài khoản hoặc mật khẩu.");
        }

        var accessToken = _jwtTokenService.CreateToken(user);

        return new
        {
            accessToken,

            refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),

            user,

            tenant = _tenants.FirstOrDefault(t => t.Id == user.TenantId)
        };
    }
    public IReadOnlyCollection<KhachHang> GetCustomers(Guid tenantId, string? search)
    {
        var query = _context.KhachHangs
            .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.Name.Contains(search) ||
                c.Phone.Contains(search) ||
                c.CitizenId.Contains(search));
        }

        return query
            .OrderByDescending(c => c.RiskScore)
            .ToArray();
    }
    public KhachHang CreateCustomer(Guid tenantId, CreateCustomerRequest request)
    {
        var customer = new KhachHang
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Phone = request.Phone,
            Address = request.Address,
            CitizenId = request.CitizenId,
            RiskScore = Random.Shared.Next(18, 88)
        };
        _context.KhachHangs.Add(customer);
        _context.SaveChanges(); AddAudit(tenantId, "Operator", "Create", "Customer");
        return customer;
    }

    public IReadOnlyCollection<Contract> GetContracts(Guid tenantId) =>
        _context.Contracts
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.StartDate)
            .ToArray();
    public IReadOnlyCollection<object> GetDebts(
        Guid tenantId,
        string? search,
        string? status,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize)
    {
        var query =
            from debt in _context.CongNos
            join contract in _context.Contracts on debt.ContractId equals contract.Id
            join customer in _context.KhachHangs on contract.CustomerId equals customer.Id
            where debt.TenantId == tenantId
            select new { debt, contract, customer };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.customer.Name.Contains(search) ||
                x.customer.Phone.Contains(search) ||
                x.contract.Code.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.debt.Status.ToString() == status);
        }

        if (from is not null)
        {
            query = query.Where(x => x.debt.DueDate >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.debt.DueDate <= to);
        }

        return query
            .OrderBy(x => x.debt.DueDate)
            .Skip(Math.Max(0, page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.debt.Id,
                x.contract.Code,
                CustomerName = x.customer.Name,
                x.customer.Phone,
                x.customer.Address,
                x.customer.RiskScore,
                x.debt.PrincipalAmount,
                x.debt.PenaltyAmount,
                x.debt.ReminderFee,
                x.debt.TotalAmount,
                x.debt.PaidAmount,
                x.debt.RemainingAmount,
                x.debt.DueDate,
                x.debt.OverdueDays,
                x.debt.Status,
                x.debt.Note
            })
            .ToArray();
    }
    public CongNo CreateDebt(Guid tenantId, CreateDebtRequest request)
    {
        var contractExists = _context.Contracts.Any(c =>
            c.Id == request.ContractId &&
            c.TenantId == tenantId &&
            !c.IsClosed);

        if (!contractExists)
        {
            throw new InvalidOperationException("Hop dong khong ton tai hoac da dong.");
        }

        var debt = new CongNo
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractId = request.ContractId,
            PrincipalAmount = request.PrincipalAmount,
            PenaltyRate = request.PenaltyRate,
            ReminderFee = request.ReminderFee,
            IssuedDate = request.IssuedDate,
            DueDate = request.DueDate,
            Note = request.Note ?? "",
            Status = TrangThaiCongNo.Active
        };

        debt.RefreshStatus();

        _context.CongNos.Add(debt);
        _context.SaveChanges();

        AddAudit(tenantId, "Operator", "Create", "Debt");

        return debt;
    }
    public ThanhToan AddPayment(Guid tenantId, Guid debtId, CreatePaymentRequest request)
    {
        var debt = _context.CongNos.FirstOrDefault(d => d.TenantId == tenantId && d.Id == debtId)
            ?? throw new InvalidOperationException("Khoan no khong ton tai.");

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("So tien thanh toan phai lon hon 0.");
        }

        if (request.Amount > debt.RemainingAmount)
        {
            throw new InvalidOperationException("So tien thanh toan vuot qua du no.");
        }

        debt.ApplyPayment(request.Amount);

        var payment = new ThanhToan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DebtId = debtId,
            Amount = request.Amount,
            Method = request.Method,
            PaidAt = DateTime.UtcNow,
            ReceivedBy = request.ReceivedBy
        };

        _context.ThanhToans.Add(payment);
        _context.SaveChanges();

        AddAudit(tenantId, request.ReceivedBy, "Create", "Payment");

        return payment;
    }

    public CollectionTask CreateTask(Guid tenantId, CreateTaskRequest request)
    {
        if (!_debts.Any(d => d.TenantId == tenantId && d.Id == request.DebtId))
        {
            throw new InvalidOperationException("Khoan no khong ton tai.");
        }

        if (!_users.Any(u => u.TenantId == tenantId && u.Id == request.AssignedTo && u.Role == UserRole.FieldCollector))
        {
            throw new InvalidOperationException("Nhan vien hien truong khong hop le.");
        }

        var task = new CollectionTask(Guid.NewGuid(), tenantId, request.DebtId, request.AssignedTo, CollectionTaskStatus.Assigned, request.DueDate, request.Note);
        _tasks.Add(task);
        AddAudit(tenantId, "Operator", "Assign", "CollectionTask");
        return task;
    }

    public NotificationLog SendReminder(Guid tenantId, Guid debtId, SendReminderRequest request)
    {
        var item =
            (from debt in _debts
             join contract in _contracts on debt.ContractId equals contract.Id
             join customer in _customers on contract.CustomerId equals customer.Id
             where debt.TenantId == tenantId && debt.Id == debtId
             select new { debt, customer }).FirstOrDefault()
            ?? throw new InvalidOperationException("Khoan no khong ton tai.");

        var message = $"Kinh gui {item.customer.Name}, quy khach con du no {item.debt.RemainingAmount:n0} VND, han thanh toan {item.debt.DueDate:dd/MM/yyyy}.";
        var notification = new NotificationLog(Guid.NewGuid(), tenantId, debtId, request.Channel, item.customer.Phone, message, "Sent", DateTime.UtcNow);
        _notifications.Add(notification);
        AddAudit(tenantId, "Operator", "SendReminder", "Notification");
        return notification;
    }

    public IReadOnlyCollection<ThanhToan> GetPayments(Guid tenantId, Guid? debtId) =>
        _context.ThanhToans
            .Where(p => p.TenantId == tenantId && (debtId == null || p.DebtId == debtId))
            .OrderByDescending(p => p.PaidAt)
            .ToArray();
    public IReadOnlyCollection<NotificationLog> GetNotifications(Guid tenantId) =>
        _notifications.Where(n => n.TenantId == tenantId).OrderByDescending(n => n.SentAt).ToArray();

    public IReadOnlyCollection<AuditLog> GetAuditLogs(Guid tenantId) =>
        _auditLogs.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.Timestamp).Take(50).ToArray();

    public object GetDashboard(Guid tenantId)
    {
        var debts = _context.CongNos
            .Where(d => d.TenantId == tenantId)
            .ToArray();

        var payments = _context.ThanhToans
            .Where(p => p.TenantId == tenantId)
            .ToArray();

        var customers = _context.KhachHangs
            .Where(c => c.TenantId == tenantId)
            .ToArray();

        var totalReceivable = debts.Sum(d => d.RemainingAmount);
        var totalPaid = payments.Sum(p => p.Amount);

        var overdue = debts
            .Where(d => d.Status == TrangThaiCongNo.Overdue)
            .ToArray();

        var dueSoon = debts
            .Where(d => d.Status == TrangThaiCongNo.DueSoon)
            .ToArray();

        var collectionRate = totalPaid + totalReceivable == 0
            ? 0
            : Math.Round(totalPaid / (totalPaid + totalReceivable) * 100, 1);

        return new
        {
            kpis = new
            {
                totalReceivable,
                totalPaid,
                overdueAmount = overdue.Sum(d => d.RemainingAmount),
                overdueCount = overdue.Length,
                dueSoonCount = dueSoon.Length,
                collectionRate
            },

            statusBreakdown = debts
                .GroupBy(d => d.Status)
                .Select(g => new
                {
                    status = g.Key.ToString(),
                    count = g.Count(),
                    amount = g.Sum(d => d.RemainingAmount)
                }),

            monthlyCollection = payments
                .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
                .Select(g => new
                {
                    month = $"{g.Key.Month:00}/{g.Key.Year}",
                    amount = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.month),

            highRiskCustomers = customers
    .Where(c => c.RiskScore >= 65)
    .OrderByDescending(c => c.RiskScore)
    .Select(c => new
    {
        c.Name,
        c.Phone,
        c.RiskScore
    }),

            upcomingAlerts = debts
    .Where(d =>
        d.Status == TrangThaiCongNo.DueSoon ||
        d.Status == TrangThaiCongNo.Overdue)
    .OrderBy(d => d.DueDate)
    .Select(d => new
    {
        Name = "Khách hàng",
        d.RemainingAmount,
        d.DueDate,
        d.Status,
        d.OverdueDays
    })
        };
    }
    private void RefreshDebtStatuses()
    {
        foreach (var debt in _debts)
        {
            debt.RefreshStatus();
        }
    }

    private void AddAudit(Guid tenantId, string userName, string action, string entity)
    {
        _auditLogs.Add(new AuditLog(Guid.NewGuid(), tenantId, userName, action, entity, DateTime.UtcNow, "127.0.0.1"));
    }

    private void ValidateUserRequest(Guid tenantId, string fullName, string email, UserRole role, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Ho ten nguoi dung la bat buoc.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Email khong hop le.");
        }

        if (role == UserRole.SuperAdmin)
        {
            throw new InvalidOperationException("Khong the tao hoac sua SuperAdmin tu man hinh tenant.");
        }

        if (_users.Any(u =>
                u.TenantId == tenantId &&
                u.Id != userId &&
                u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Email da ton tai trong tenant.");
        }
    }

    private void Seed()
    {
        var tenant = new Tenant(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Cong ty Tai chinh Pacific", "Pro", TenantStatus.Active, DateTime.UtcNow.AddMonths(-6));
        _tenants.Add(tenant);
        _tenantAliases["demo"] = tenant.Id;

        var operatorUser = new AppUser(Guid.NewGuid(), tenant.Id, "Le Dinh Anh Tuan", "operator@demo.vn", UserRole.Operator, true);
        var adminUser = new AppUser(Guid.NewGuid(), tenant.Id, "Quan tri Tenant", "admin@demo.vn", UserRole.TenantAdmin, true);
        var collector = new AppUser(Guid.NewGuid(), tenant.Id, "Nguyen Van Field", "field@demo.vn", UserRole.FieldCollector, true);
        _users.AddRange([operatorUser, adminUser, collector, new AppUser(Guid.NewGuid(), tenant.Id, "Khach hang Demo", "customer@demo.vn", UserRole.Customer, true)]);

        var customers = new[]
        {
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Nguyen Van An", Phone = "0901000001", Address = "Nha Trang, Khanh Hoa", CitizenId = "056201000001", RiskScore = 78 },
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Tran Thi Bich", Phone = "0901000002", Address = "Cam Ranh, Khanh Hoa", CitizenId = "056201000002", RiskScore = 42 },
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Pham Minh Chau", Phone = "0901000003", Address = "Dien Khanh, Khanh Hoa", CitizenId = "056201000003", RiskScore = 66 },
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Hoang Gia Huy", Phone = "0901000004", Address = "Van Ninh, Khanh Hoa", CitizenId = "056201000004", RiskScore = 24 }
        };
        _customers.AddRange(customers);

        foreach (var customer in customers)
        {
            _contracts.Add(new Contract(Guid.NewGuid(), tenant.Id, customer.Id, $"HD-{customer.Phone[^4..]}", Random.Shared.Next(30, 140) * 1_000_000m, 1.4m, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)), DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(9)), false));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var contractList = _contracts.ToArray();
        _debts.AddRange([
            NewDebt(tenant.Id, contractList[0].Id, 28_000_000m, today.AddDays(-15), "Qua han can uu tien goi dien"),
            NewDebt(tenant.Id, contractList[1].Id, 15_500_000m, today.AddDays(3), "Sap den han D-3"),
            NewDebt(tenant.Id, contractList[2].Id, 42_000_000m, today.AddDays(-34), "Rui ro cao, can giao field collector"),
            NewDebt(tenant.Id, contractList[3].Id, 9_800_000m, today.AddDays(18), "Dang theo doi")
        ]);

        AddPayment(tenant.Id, _debts[1].Id, new CreatePaymentRequest(5_000_000m, PhuongThucThanhToan.BankTransfer, operatorUser.FullName));
        _tasks.Add(new CollectionTask(Guid.NewGuid(), tenant.Id, _debts[2].Id, collector.Id, CollectionTaskStatus.Assigned, today.AddDays(1), "Gap khach va xac minh kha nang thanh toan."));
        SendReminder(tenant.Id, _debts[0].Id, new SendReminderRequest("SMS"));
    }

    private static CongNo NewDebt(Guid tenantId, Guid contractId, decimal amount, DateOnly dueDate, string note)
    {
        var debt = new CongNo
        {
            TenantId = tenantId,
            ContractId = contractId,
            PrincipalAmount = amount,
            PenaltyRate = 2.5m,
            ReminderFee = 20_000m,
            IssuedDate = dueDate.AddMonths(-1),
            DueDate = dueDate,
            Note = note,
            Status = TrangThaiCongNo.Active
        };
        debt.RefreshStatus();
        return debt;
    }
}

