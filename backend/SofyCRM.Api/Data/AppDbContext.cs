using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>             Users             => Set<User>();
    public DbSet<Session>          Sessions          => Set<Session>();
    public DbSet<Customer>         Customers         => Set<Customer>();
    public DbSet<CustomerContact>  CustomerContacts  => Set<CustomerContact>();
    public DbSet<CustomerFollowup> CustomerFollowups => Set<CustomerFollowup>();
    public DbSet<Opportunity>      Opportunities     => Set<Opportunity>();
    public DbSet<Quotation>        Quotations        => Set<Quotation>();
    public DbSet<QuotationItem>    QuotationItems    => Set<QuotationItem>();
    public DbSet<Project>          Projects          => Set<Project>();
    public DbSet<ProjectTask>      ProjectTasks      => Set<ProjectTask>();
    public DbSet<Ticket>           Tickets           => Set<Ticket>();
    public DbSet<WorkLog>          WorkLogs          => Set<WorkLog>();
    public DbSet<Expense>          Expenses          => Set<Expense>();
    public DbSet<Contract>         Contracts         => Set<Contract>();
    public DbSet<Invoice>          Invoices          => Set<Invoice>();
    public DbSet<Notification>     Notifications     => Set<Notification>();
    public DbSet<AuditLog>         AuditLogs         => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.HasPostgresExtension("uuid-ossp");
        b.HasPostgresExtension("pgcrypto");

        // PostgreSQL enums  (must match SQL schema)
        b.HasPostgresEnum<UserRole>("user_role");
        b.HasPostgresEnum<UserStatus>("user_status");
        b.HasPostgresEnum<CustomerStatus>("customer_status");
        b.HasPostgresEnum<FollowupType>("followup_type");
        b.HasPostgresEnum<OpportunityStatus>("opportunity_status");
        b.HasPostgresEnum<QuotationStatus>("quotation_status");
        b.HasPostgresEnum<ProjectStatus>("project_status");
        b.HasPostgresEnum<Entities.TaskStatus>("task_status");
        b.HasPostgresEnum<TicketStatus>("ticket_status");
        b.HasPostgresEnum<TicketPriority>("ticket_priority");
        b.HasPostgresEnum<ExpenseCategory>("expense_category");
        b.HasPostgresEnum<ExpenseStatus>("expense_status");
        b.HasPostgresEnum<PaymentStatus>("payment_status");

        // ---------------- Users ----------------
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });

        // ---------------- Sessions ----------------
        b.Entity<Session>(e =>
        {
            e.HasIndex(x => x.RefreshToken).IsUnique();
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Customer ----------------
        b.Entity<Customer>(e =>
        {
            e.Property(x => x.Tags).HasColumnType("text[]");
            e.HasOne(x => x.OwnerUser)
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<CustomerContact>(e =>
        {
            e.HasOne(x => x.Customer)
                .WithMany(c => c.Contacts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CustomerFollowup>(e =>
        {
            e.HasOne(x => x.Customer)
                .WithMany(c => c.Followups)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        b.Entity<Opportunity>(e =>
        {
            e.HasOne(x => x.Customer)
                .WithMany(c => c.Opportunities)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.OwnerUser).WithMany().HasForeignKey(x => x.OwnerUserId);
        });

        // ---------------- Quotation ----------------
        b.Entity<Quotation>(e =>
        {
            e.HasIndex(x => x.QuotationNo).IsUnique();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Opportunity).WithMany().HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<QuotationItem>(e =>
        {
            e.HasOne(x => x.Quotation)
                .WithMany(q => q.Items)
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Project ----------------
        b.Entity<Project>(e =>
        {
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PmUser).WithMany().HasForeignKey(x => x.PmUserId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<ProjectTask>(e =>
        {
            e.HasOne(x => x.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Ticket>(e =>
        {
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<WorkLog>(e =>
        {
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        b.Entity<Expense>(e =>
        {
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Contract>(e =>
        {
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.OwnerUser).WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Invoice>(e =>
        {
            e.HasIndex(x => x.InvoiceNo).IsUnique();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Notification>(e =>
        {
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<AuditLog>(e =>
        {
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.Module);
        });
    }
}
