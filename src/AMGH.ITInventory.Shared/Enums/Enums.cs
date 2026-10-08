namespace AMGH.ITInventory.Shared.Enums;

public enum AssetStatus { InStock = 1, InUse = 2, InRepair = 3, Lost = 4, Retired = 5, Disposed = 6 }
public enum AssetCondition { New = 1, Good = 2, Fair = 3, Poor = 4, Damaged = 5 }
/// <summary>ISO 27001 Annex A.5.12 information classification.</summary>
public enum SecurityClassification { Public = 1, Internal = 2, Confidential = 3, Restricted = 4 }
public enum CampaignStatus { Draft = 1, Active = 2, UnderReview = 3, Completed = 4, Cancelled = 5 }
public enum VerificationMethod { Qr = 1, Barcode = 2, Manual = 3, Discovery = 4 }
public enum VerificationResult { Pending = 1, Verified = 2, Missing = 3, LocationMismatch = 4, CustodianMismatch = 5, Damaged = 6 }
public enum ApprovalStatus { Pending = 1, Approved = 2, Rejected = 3 }
public enum MaintenanceType { Preventive = 1, Corrective = 2, Upgrade = 3 }
public enum MaintenanceStatus { Scheduled = 1, InProgress = 2, Completed = 3, Cancelled = 4 }
public enum RequestStatus { Draft = 1, Submitted = 2, Approved = 3, Rejected = 4, Ordered = 5, Received = 6, Closed = 7 }
public enum BudgetType { Capex = 1, Opex = 2 }
public enum MovementType { Assign = 1, Return = 2, Transfer = 3, Repair = 4, Dispose = 5 }
