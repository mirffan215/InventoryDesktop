namespace AMGH.ITInventory.Domain.Common;

public abstract class Entity
{
    public int Id { get; set; }
}

/// <summary>ISO 27001 traceability: who/when created and modified, plus soft delete.</summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
