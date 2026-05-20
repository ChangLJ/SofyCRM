using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SofyCRM.Api.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(255)] public string Email { get; set; } = string.Empty;
    [MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Sales;
    public UserStatus Status { get; set; } = UserStatus.Active;
    [MaxLength(50)] public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(512)] public string RefreshToken { get; set; } = string.Empty;
    [MaxLength(512)] public string? UserAgent { get; set; }
    [MaxLength(64)]  public string? IpAddress { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(200)] public string CompanyName { get; set; } = string.Empty;
    [MaxLength(50)]  public string? TaxId { get; set; }
    public string? Address { get; set; }
    [MaxLength(100)] public string? Industry { get; set; }
    public Guid? OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Potential;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<CustomerContact> Contacts { get; set; } = new();
    public List<CustomerFollowup> Followups { get; set; } = new();
    public List<Opportunity> Opportunities { get; set; } = new();
}

public class CustomerContact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(100)] public string? Title { get; set; }
    [MaxLength(50)]  public string? Phone { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CustomerFollowup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public FollowupType FollowupType { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime? NextFollowupDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Opportunity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [Column(TypeName = "numeric(18,2)")] public decimal Amount { get; set; }
    public OpportunityStatus Status { get; set; } = OpportunityStatus.NewLead;
    public DateOnly? ExpectedCloseDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Quotation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    [MaxLength(50)]  public string QuotationNo { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    [Column(TypeName = "numeric(18,2)")] public decimal TotalAmount { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
    public DateOnly? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<QuotationItem> Items { get; set; } = new();
}

public class QuotationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuotationId { get; set; }
    public Quotation? Quotation { get; set; }
    [MaxLength(200)] public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Qty { get; set; } = 1;
    [Column(TypeName = "numeric(18,2)")] public decimal UnitPrice { get; set; }
    [Column(TypeName = "numeric(10,2)")] public decimal EstimatedHours { get; set; }
    public int SortOrder { get; set; }
}

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    [MaxLength(200)] public string ProjectName { get; set; } = string.Empty;
    public Guid? PmUserId { get; set; }
    public User? PmUser { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<ProjectTask> Tasks { get; set; } = new();
}

public class ProjectTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    [Column(TypeName = "numeric(10,2)")] public decimal EstimatedHours { get; set; }
    [Column(TypeName = "numeric(10,2)")] public decimal ActualHours { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime? SlaDueAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateOnly WorkDate { get; set; }
    [Column(TypeName = "numeric(6,2)")] public decimal Hours { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ExpenseCategory Category { get; set; }
    [Column(TypeName = "numeric(18,2)")] public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    [MaxLength(500)] public string? ReceiptUrl { get; set; }
    public string? Description { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Contract
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    [MaxLength(200)] public string ContractName { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int RenewalNoticeDays { get; set; } = 30;
    [MaxLength(500)] public string? FileUrl { get; set; }
    public string? Notes { get; set; }
    public Guid? OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    [MaxLength(50)]  public string InvoiceNo { get; set; } = string.Empty;
    [Column(TypeName = "numeric(18,2)")] public decimal Amount { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    [Column(TypeName = "numeric(18,2)")] public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    [MaxLength(50)]  public string Type { get; set; } = "System";
    public bool IsRead { get; set; }
    [MaxLength(500)] public string? Link { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    [MaxLength(100)] public string Module { get; set; } = string.Empty;
    [MaxLength(100)] public string Action { get; set; } = string.Empty;
    [MaxLength(100)] public string? EntityId { get; set; }
    [Column(TypeName = "jsonb")] public string? BeforeData { get; set; }
    [Column(TypeName = "jsonb")] public string? AfterData { get; set; }
    [MaxLength(64)]  public string? IpAddress { get; set; }
    [MaxLength(512)] public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
