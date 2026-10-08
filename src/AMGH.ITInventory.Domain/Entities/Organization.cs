using AMGH.ITInventory.Domain.Common;

namespace AMGH.ITInventory.Domain.Entities;

public class Department : AuditableEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int? ParentDepartmentId { get; set; }
    public Department? Parent { get; set; }
    public string? EntraGroupId { get; set; }
}

public class Location : AuditableEntity
{
    public string Building { get; set; } = "";
    public string? Floor { get; set; }
    public string? Room { get; set; }
    public string FullName => string.Join(" / ", new[] { Building, Floor, Room }.Where(s => !string.IsNullOrWhiteSpace(s)));
}

public class Employee : AuditableEntity
{
    public string EmployeeNumber { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Email { get; set; }
    public string? EntraObjectId { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Vendor : AuditableEntity
{
    public string Name { get; set; } = "";
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
    public bool IsApproved { get; set; }
}
