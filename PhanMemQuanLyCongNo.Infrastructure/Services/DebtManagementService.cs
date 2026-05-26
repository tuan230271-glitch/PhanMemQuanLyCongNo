using System.Collections.Concurrent;
using TaskStatus = PhanMemQuanLyCongNo.Application.Models.CollectionTaskStatus;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Models;
using PhanMemQuanLyCongNo.Domain.Entities;
using PhanMemQuanLyCongNo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using PhanMemQuanLyCongNo.Infrastructure.Persistence.DbContext;
using DomainContract = PhanMemQuanLyCongNo.Domain.Entities.Contract;
using PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Update;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Update_Status;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Add_Payment;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Send_Reminder;
using PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Update_Status;
using PhanMemQuanLyCongNo.Application.Features.Users.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Users.Commands.Update;
using PhanMemQuanLyCongNo.Application.Features.Auth.Commands.Login;

namespace PhanMemQuanLyCongNo.Infrastructure.Services;

public class DebtManagementService : IDebtManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private static readonly object SeedLock = new();
    private static readonly List<CollectionTask> Tasks = [];
    private static readonly List<NotificationLog> Notifications = [];
    private static readonly List<AuditLog> AuditLogs = [];
    private static readonly ConcurrentDictionary<string, Guid> TenantAliases = new(StringComparer.OrdinalIgnoreCase);

    public DebtManagementService(
        ApplicationDbContext context,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        EnsureSeeded();
    }

    public Guid DefaultTenantId =>
        _context.Tenants.Select(t => t.Id).FirstOrDefault() is var id && id != Guid.Empty
            ? id
            : Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid ResolveTenant(string? tenantId)
    {
        if (Guid.TryParse(tenantId, out var parsed) && _context.Tenants.Any(t => t.Id == parsed))
        {
            return parsed;
        }

        if (!string.IsNullOrWhiteSpace(tenantId) && TenantAliases.TryGetValue(tenantId, out var alias))
        {
            return alias;
        }

        return DefaultTenantId;
    }

    public IReadOnlyCollection<Tenant> GetTenants() =>
        _context.Tenants.OrderBy(t => t.Name).ToArray();

    public IReadOnlyCollection<AppUser> GetUsers(Guid tenantId) =>
        _context.Users
            .Where(u => u.TenantId == tenantId || u.Role == UserRole.SuperAdmin)
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FullName)
            .ToArray();

    public AppUser CreateUser(Guid tenantId, CreateUserRequest request)
    {
        ValidateUserRequest(tenantId, request.FullName, request.Email, request.Role);

        var user = new AppUser(Guid.NewGuid(), tenantId, request.FullName.Trim(), request.Email.Trim(), request.Role, request.IsActive)
        {
            PasswordHash = PasswordHasher.Hash("123456")
        };
        _context.Users.Add(user);
        _context.SaveChanges();
        AddAudit(tenantId, "TenantAdmin", "Create", "User");
        return user;
    }

    public AppUser UpdateUser(Guid tenantId, Guid userId, UpdateUserRequest request)
    {
        var current = _context.Users.FirstOrDefault(u => u.Id == userId && u.TenantId == tenantId)
            ?? throw new InvalidOperationException("Nguoi dung khong ton tai.");

        ValidateUserRequest(tenantId, request.FullName, request.Email, request.Role, userId);

        current.FullName = request.FullName.Trim();
        current.Email = request.Email.Trim();
        current.Role = request.Role;
        current.IsActive = request.IsActive;
        _context.SaveChanges();
        AddAudit(tenantId, "TenantAdmin", "Update", "User");
        return current;
    }

    public void DeleteUser(Guid tenantId, Guid userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId && u.TenantId == tenantId)
            ?? throw new InvalidOperationException("Nguoi dung khong ton tai.");

        if (Tasks.Any(t => t.TenantId == tenantId && t.AssignedTo == userId))
        {
            throw new InvalidOperationException("Nguoi dung da duoc gan cong viec, hay khoa tai khoan thay vi xoa.");
        }

        _context.Users.Remove(user);
        _context.SaveChanges();
        AddAudit(tenantId, "TenantAdmin", "Delete", "User");
    }

    public object Login(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();
        var user = _context.Users
            .FirstOrDefault(u =>
                u.Email.ToLower() == normalizedEmail
                && u.IsActive);

        if (user is null)
        {
            throw new InvalidOperationException("Sai tài khoản hoặc mật khẩu.");
        }

        if (!string.Equals(user.PasswordHash, PasswordHasher.Hash(request.Password), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Sai tài khoản hoặc mật khẩu.");
        }

        var accessToken = _jwtTokenService.CreateToken(user);

        return new
        {
            accessToken,

            refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),

            user,

            tenant = _context.Tenants.FirstOrDefault(t => t.Id == user.TenantId)
        };
    }
    public IReadOnlyCollection<KhachHang> GetCustomers(Guid tenantId, string? search)
    {
        var query = _context.KhachHangs
            .Where(c => c.TenantId == tenantId && !c.IsDeleted);
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
    public KhachHang? GetCustomerById(Guid tenantId, Guid customerId)
    {
        return _context.KhachHangs
           .FirstOrDefault(c => c.TenantId == tenantId && c.Id == customerId && !c.IsDeleted);
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

    public KhachHang UpdateCustomer(Guid tenantId, Guid customerId, UpdateCustomerRequest request)
    {
        var customer = _context.KhachHangs
            .FirstOrDefault(c => c.TenantId == tenantId && c.Id == customerId)
            ?? throw new InvalidOperationException("Khach hang khong ton tai.");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Ten khach hang la bat buoc.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new InvalidOperationException("So dien thoai la bat buoc.");
        }

        if (string.IsNullOrWhiteSpace(request.CitizenId))
        {
            throw new InvalidOperationException("CCCD/CMND la bat buoc.");
        }

        customer.Name = request.Name.Trim();
        customer.Phone = request.Phone.Trim();
        customer.Address = request.Address.Trim();
        customer.CitizenId = request.CitizenId.Trim();

        _context.SaveChanges();

        AddAudit(tenantId, "Operator", "Update", "Customer");

        return customer;
    }
    public void DeleteCustomer(Guid tenantId, Guid customerId)
    {
        var customer = _context.KhachHangs
            .FirstOrDefault(c => c.TenantId == tenantId && c.Id == customerId && !c.IsDeleted)
            ?? throw new InvalidOperationException("Khach hang khong ton tai.");

        customer.IsDeleted = true;

        _context.SaveChanges();

        AddAudit(tenantId, "Operator", "Delete", "Customer");
    }

    public IReadOnlyCollection<DomainContract> GetContracts(Guid tenantId) =>
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
    public object? GetDebtById(Guid tenantId, Guid debtId)
    {
        var item =
            (from debt in _context.CongNos
             join contract in _context.Contracts on debt.ContractId equals contract.Id
             join customer in _context.KhachHangs on contract.CustomerId equals customer.Id
             where debt.TenantId == tenantId && debt.Id == debtId
             select new
             {
                 debt.Id,
                 ContractId = contract.Id,
                 ContractCode = contract.Code,
                 CustomerId = customer.Id,
                 CustomerName = customer.Name,
                 customer.Phone,
                 customer.Address,
                 customer.CitizenId,
                 customer.RiskScore,
                 debt.PrincipalAmount,
                 debt.PenaltyAmount,
                 debt.ReminderFee,
                 debt.TotalAmount,
                 debt.PaidAmount,
                 debt.RemainingAmount,
                 debt.IssuedDate,
                 debt.DueDate,
                 debt.OverdueDays,
                 debt.Status,
                 debt.Note
             })
            .FirstOrDefault();

        return item;
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

    public CongNo UpdateDebtStatus(Guid tenantId, Guid debtId, UpdateDebtStatusRequest request)
    {
        var debt = _context.CongNos
            .FirstOrDefault(d => d.TenantId == tenantId && d.Id == debtId)
            ?? throw new InvalidOperationException("Khoan no khong ton tai.");

        debt.Status = request.Status;
        _context.SaveChanges();

        AddAudit(tenantId, "Operator", "UpdateStatus", "Debt");

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
        if (!_context.CongNos.Any(d => d.TenantId == tenantId && d.Id == request.DebtId))
        {
            throw new InvalidOperationException("Khoan no khong ton tai.");
        }

        if (!_context.Users.Any(u => u.TenantId == tenantId && u.Id == request.AssignedTo && u.Role == UserRole.FieldCollector))
        {
            throw new InvalidOperationException("Nhan vien hien truong khong hop le.");
        }

        var task = new CollectionTask(
            Guid.NewGuid(),
            tenantId,
            request.DebtId,
            request.AssignedTo,
            TaskStatus.Assigned,
            request.DueDate,
            request.Note ?? ""
        );
        Tasks.Add(task);

        AddAudit(tenantId, "Operator", "Assign", "CollectionTask");

        return task;
    }
    public IReadOnlyCollection<CollectionTask> GetTasks(Guid tenantId)
    {
        return Tasks
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.DueDate)
            .ToArray();
    }

    public CollectionTask? GetTaskById(Guid tenantId, Guid taskId)
    {
        return Tasks
            .FirstOrDefault(t => t.TenantId == tenantId && t.Id == taskId);
    }

    public CollectionTask UpdateTaskStatus(Guid tenantId, Guid taskId, UpdateTaskStatusRequest request)
    {
        var index = Tasks.FindIndex(t => t.TenantId == tenantId && t.Id == taskId);

        if (index < 0)
        {
            throw new InvalidOperationException("Cong viec thu hoi no khong ton tai.");
        }

        var currentTask = Tasks[index];

        var updatedTask = currentTask with
        {
            Status = request.Status
        };

        Tasks[index] = updatedTask;

        AddAudit(tenantId, "Operator", "UpdateStatus", "CollectionTask");

        return updatedTask;
    }
    public NotificationLog SendReminder(Guid tenantId, Guid debtId, SendReminderRequest request)
    {
        var item =
            (from debt in _context.CongNos
             join contract in _context.Contracts on debt.ContractId equals contract.Id
             join customer in _context.KhachHangs on contract.CustomerId equals customer.Id
             where debt.TenantId == tenantId && debt.Id == debtId
             select new { debt, customer }).FirstOrDefault()
            ?? throw new InvalidOperationException("Khoan no khong ton tai.");

        var message = $"Kinh gui {item.customer.Name}, quy khach con du no {item.debt.RemainingAmount:n0} VND, han thanh toan {item.debt.DueDate:dd/MM/yyyy}.";
        var notification = new NotificationLog(Guid.NewGuid(), tenantId, debtId, request.Channel, item.customer.Phone, message, "Sent", DateTime.UtcNow);
        Notifications.Add(notification);
        AddAudit(tenantId, "Operator", "SendReminder", "Notification");
        return notification;
    }

    public IReadOnlyCollection<ThanhToan> GetPayments(Guid tenantId, Guid? debtId) =>
        _context.ThanhToans
            .Where(p => p.TenantId == tenantId && (debtId == null || p.DebtId == debtId))
            .OrderByDescending(p => p.PaidAt)
            .ToArray();
    public IReadOnlyCollection<NotificationLog> GetNotifications(Guid tenantId) =>
        Notifications.Where(n => n.TenantId == tenantId).OrderByDescending(n => n.SentAt).ToArray();

    public IReadOnlyCollection<AuditLog> GetAuditLogs(Guid tenantId) =>
        AuditLogs.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.Timestamp).Take(50).ToArray();

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
        foreach (var debt in _context.CongNos)
        {
            debt.RefreshStatus();
        }

        _context.SaveChanges();
    }

    private void AddAudit(Guid tenantId, string userName, string action, string entity)
    {
        AuditLogs.Add(new AuditLog(Guid.NewGuid(), tenantId, userName, action, entity, DateTime.UtcNow, "127.0.0.1"));
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

        var normalizedEmail = email.Trim().ToLower();
        if (_context.Users.Any(u =>
                u.TenantId == tenantId &&
                u.Id != userId &&
                u.Email.ToLower() == normalizedEmail))
        {
            throw new InvalidOperationException("Email da ton tai trong tenant.");
        }
    }

    private void EnsureSeeded()
    {
        TenantAliases["demo"] = Guid.Parse("11111111-1111-1111-1111-111111111111");

        if (_context.Tenants.Any())
        {
            return;
        }

        lock (SeedLock)
        {
            if (_context.Tenants.Any())
            {
                return;
            }

        var tenant = new Tenant(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Cong ty Tai chinh Pacific", "Pro", TenantStatus.Active, DateTime.UtcNow.AddMonths(-6));
        _context.Tenants.Add(tenant);

        var operatorUser = new AppUser(Guid.NewGuid(), tenant.Id, "Le Dinh Anh Tuan", "operator@demo.vn", UserRole.Operator, true);
        var adminUser = new AppUser(Guid.NewGuid(), tenant.Id, "Quan tri Tenant", "admin@demo.vn", UserRole.TenantAdmin, true);
        var collector = new AppUser(Guid.NewGuid(), tenant.Id, "Nguyen Van Field", "field@demo.vn", UserRole.FieldCollector, true);
        var customerUser = new AppUser(Guid.NewGuid(), tenant.Id, "Khach hang Demo", "customer@demo.vn", UserRole.Customer, true);

        foreach (var user in new[] { operatorUser, adminUser, collector, customerUser })
        {
            user.PasswordHash = PasswordHasher.Hash("123456");
        }

        _context.Users.AddRange(operatorUser, adminUser, collector, customerUser);

        var customers = new[]
        {
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Nguyen Van An", Phone = "0901000001", Address = "Nha Trang, Khanh Hoa", CitizenId = "056201000001", RiskScore = 78 },
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Tran Thi Bich", Phone = "0901000002", Address = "Cam Ranh, Khanh Hoa", CitizenId = "056201000002", RiskScore = 42 },
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Pham Minh Chau", Phone = "0901000003", Address = "Dien Khanh, Khanh Hoa", CitizenId = "056201000003", RiskScore = 66 },
            new KhachHang { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Hoang Gia Huy", Phone = "0901000004", Address = "Van Ninh, Khanh Hoa", CitizenId = "056201000004", RiskScore = 24 }
        };
        _context.KhachHangs.AddRange(customers);

        var contracts = new List<DomainContract>();

        foreach (var customer in customers)
        {
            contracts.Add(new DomainContract
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                CustomerId = customer.Id,
                Code = $"HD-{customer.Phone[^4..]}",
                Amount = Random.Shared.Next(30, 140) * 1_000_000m,
                InterestRate = 1.4m,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(9)),
                IsClosed = false
            });
        }

        _context.Contracts.AddRange(contracts);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var contractList = contracts.ToArray();
        var debts = new[]
        {
            NewDebt(tenant.Id, contractList[0].Id, 28_000_000m, today.AddDays(-15), "Qua han can uu tien goi dien"),
            NewDebt(tenant.Id, contractList[1].Id, 15_500_000m, today.AddDays(3), "Sap den han D-3"),
            NewDebt(tenant.Id, contractList[2].Id, 42_000_000m, today.AddDays(-34), "Rui ro cao, can giao field collector"),
            NewDebt(tenant.Id, contractList[3].Id, 9_800_000m, today.AddDays(18), "Dang theo doi")
        };

        _context.CongNos.AddRange(debts);

        debts[1].ApplyPayment(5_000_000m);
        _context.ThanhToans.Add(new ThanhToan
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            DebtId = debts[1].Id,
            Amount = 5_000_000m,
            Method = PhuongThucThanhToan.BankTransfer,
            PaidAt = DateTime.UtcNow,
            ReceivedBy = operatorUser.FullName
        });

        _context.SaveChanges();

            Tasks.Add(new CollectionTask(
                Guid.NewGuid(),
                tenant.Id,
                debts[2].Id,
                collector.Id,
                TaskStatus.Assigned,
                today.AddDays(1),
                "Gap khach va xac minh kha nang thanh toan."
            ));
        Notifications.Add(new NotificationLog(Guid.NewGuid(), tenant.Id, debts[0].Id, "SMS", customers[0].Phone, $"Kinh gui {customers[0].Name}, quy khach con du no {debts[0].RemainingAmount:n0} VND, han thanh toan {debts[0].DueDate:dd/MM/yyyy}.", "Sent", DateTime.UtcNow));
        AddAudit(tenant.Id, "System", "Seed", "DemoData");
        }
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

