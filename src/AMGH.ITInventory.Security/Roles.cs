namespace AMGH.ITInventory.Security;

public static class Roles
{
    public const string Admin = "Admin";
    public const string ITManager = "ITManager";
    public const string Technician = "Technician";
    public const string Auditor = "Auditor";
    public const string Custodian = "Custodian";
    public const string Approver = "Approver";
    public static readonly string[] All = { Admin, ITManager, Technician, Auditor, Custodian, Approver };
}

public static class Policies
{
    public const string ManageAssets = nameof(ManageAssets);
    public const string RunVerification = nameof(RunVerification);
    public const string ApproveVerification = nameof(ApproveVerification);
    public const string ViewReports = nameof(ViewReports);
    public const string Administer = nameof(Administer);
}
