using AMGH.ITInventory.Domain.Common;
using AMGH.ITInventory.Shared.Enums;

namespace AMGH.ITInventory.Domain.Entities;

public class Budget : AuditableEntity
{
    public int FiscalYear { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public BudgetType Type { get; set; }
    public decimal Allocated { get; set; }
    public decimal Committed { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => Allocated - Committed - Spent;

    public void Commit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > Remaining) throw new InvalidOperationException("Insufficient budget.");
        Committed += amount;
    }
}

public class PurchaseRequest : AuditableEntity
{
    public string Number { get; set; } = "";
    public int DepartmentId { get; set; }
    public int BudgetId { get; set; }
    public string Justification { get; set; } = "";
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public ICollection<PurchaseRequestLine> Lines { get; set; } = new List<PurchaseRequestLine>();
    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
}

public class PurchaseRequestLine : Entity
{
    public int PurchaseRequestId { get; set; }
    public string Description { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PurchaseOrder : AuditableEntity
{
    public string Number { get; set; } = "";
    public int PurchaseRequestId { get; set; }
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Ordered;
    public decimal Amount { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? InvoiceNumber { get; set; }
}
