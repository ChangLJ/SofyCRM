namespace SofyCRM.Api.Entities;

public enum UserRole          { Admin, Sales, Service }
public enum UserStatus        { Active, Disabled }
public enum CustomerStatus    { Potential, Contacting, Quoting, Won, Lost, Maintenance }
public enum FollowupType      { Call, Email, Meeting, Visit, Line }
public enum OpportunityStatus { NewLead, Contacted, Proposal, Negotiation, Won, Lost }
public enum QuotationStatus   { Draft, Sent, Accepted, Rejected, Expired }
public enum ProjectStatus     { Planning, Development, Testing, UAT, Completed, Maintenance }
public enum TaskStatus        { Todo, InProgress, Done, Blocked }
public enum TicketStatus      { Open, Processing, WaitingCustomer, Closed }
public enum TicketPriority    { Low, Medium, High, Critical }
public enum ExpenseCategory   { Meal, Transportation, Parking, Gift, Hotel, Other }
public enum ExpenseStatus     { Draft, Submitted, Approved, Rejected, Paid }
public enum PaymentStatus     { Pending, PartialPaid, Paid, Overdue }
