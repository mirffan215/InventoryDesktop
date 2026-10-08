USE AMGH_ITInventory;
GO
CREATE OR ALTER VIEW dbo.vw_AssetRegister AS
SELECT a.Id, a.AssetTag, a.Name, c.Name AS Category, a.SerialNumber, a.Status, a.Classification, d.Name AS Department,
       CONCAT(l.Building, ' / ', l.Room) AS Location, o.DisplayName AS Owner, cu.DisplayName AS Custodian,
       a.PurchaseDate, a.PurchaseCost, a.WarrantyEnd, a.LastVerifiedUtc
FROM dbo.Assets a
JOIN dbo.AssetCategories c ON c.Id = a.CategoryId AND c.IsDeleted = 0
LEFT JOIN dbo.Departments d ON d.Id = a.DepartmentId
LEFT JOIN dbo.Locations l ON l.Id = a.LocationId
LEFT JOIN dbo.Employees o ON o.Id = a.OwnerEmployeeId
LEFT JOIN dbo.Employees cu ON cu.Id = a.CustodianEmployeeId
WHERE a.IsDeleted = 0;
GO
CREATE OR ALTER VIEW dbo.vw_WarrantyExpiring AS
SELECT AssetTag, Name, WarrantyEnd, DATEDIFF(DAY, SYSUTCDATETIME(), WarrantyEnd) AS DaysLeft
FROM dbo.Assets WHERE IsDeleted = 0 AND WarrantyEnd IS NOT NULL AND WarrantyEnd <= DATEADD(DAY, 90, SYSUTCDATETIME());
GO
CREATE OR ALTER VIEW dbo.vw_VerificationSummary AS
SELECT c.Id AS CampaignId, c.Name, c.Status, COUNT(i.Id) AS TotalItems,
       SUM(CASE WHEN i.Result = 2 THEN 1 ELSE 0 END) AS Verified,
       SUM(CASE WHEN i.Result IN (3,4,5,6) THEN 1 ELSE 0 END) AS Exceptions,
       SUM(CASE WHEN i.Result = 1 THEN 1 ELSE 0 END) AS Pending
FROM dbo.VerificationCampaigns c LEFT JOIN dbo.VerificationItems i ON i.CampaignId = c.Id AND i.IsDeleted = 0
WHERE c.IsDeleted = 0 GROUP BY c.Id, c.Name, c.Status;
GO
CREATE OR ALTER FUNCTION dbo.fn_NetBookValue(@Cost DECIMAL(18,2), @PurchaseDate DATETIME2, @UsefulLifeMonths INT)
RETURNS DECIMAL(18,2) AS
BEGIN  -- straight-line depreciation, floored at zero
    DECLARE @m INT = DATEDIFF(MONTH, @PurchaseDate, SYSUTCDATETIME());
    IF @Cost IS NULL OR @PurchaseDate IS NULL OR @UsefulLifeMonths <= 0 RETURN NULL;
    RETURN CASE WHEN @m >= @UsefulLifeMonths THEN 0 ELSE ROUND(@Cost * (1 - CAST(@m AS DECIMAL(18,6)) / @UsefulLifeMonths), 2) END;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_MarkOverdueCampaigns AS
BEGIN
    SET NOCOUNT ON;
    -- Active campaigns past their end date move to review so approvers are prompted.
    UPDATE dbo.VerificationCampaigns SET Status = 3, ModifiedUtc = SYSUTCDATETIME(), ModifiedBy = 'sql-agent'
    WHERE Status = 2 AND EndDate < CAST(SYSUTCDATETIME() AS DATE) AND IsDeleted = 0;
    SELECT @@ROWCOUNT AS CampaignsMoved;
END
GO
CREATE OR ALTER PROCEDURE dbo.usp_PurgeOldAuditLogs @RetainDays INT = 2555 AS  -- ~7 years; align with retention policy
BEGIN
    SET NOCOUNT ON;
    IF @RetainDays < 365 THROW 50001, 'Retention below 365 days is not permitted.', 1;
    -- Requires db_owner / dedicated maintenance login: amgh_app is denied delete on AuditLogs.
    DELETE FROM dbo.AuditLogs WHERE TimestampUtc < DATEADD(DAY, -@RetainDays, SYSUTCDATETIME());
END
GO
