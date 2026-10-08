IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [AssetCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [UsefulLifeMonths] int NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_AssetCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [TimestampUtc] datetime2 NOT NULL,
        [UserName] nvarchar(max) NULL,
        [EntityName] nvarchar(450) NOT NULL,
        [EntityKey] nvarchar(450) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ParentDepartmentId] int NULL,
        [ParentId] int NULL,
        [EntraGroupId] nvarchar(max) NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Departments_Departments_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [Locations] (
        [Id] int NOT NULL IDENTITY,
        [Building] nvarchar(max) NOT NULL,
        [Floor] nvarchar(max) NULL,
        [Room] nvarchar(max) NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseRequests] (
        [Id] int NOT NULL IDENTITY,
        [Number] nvarchar(450) NOT NULL,
        [DepartmentId] int NOT NULL,
        [BudgetId] int NOT NULL,
        [Justification] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PurchaseRequests] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [SoftwareLicenses] (
        [Id] int NOT NULL IDENTITY,
        [Product] nvarchar(max) NOT NULL,
        [LicenseKeyHash] nvarchar(max) NULL,
        [TotalSeats] int NOT NULL,
        [UsedSeats] int NOT NULL,
        [ExpiryDate] datetime2 NULL,
        [VendorId] int NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_SoftwareLicenses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [Vendors] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [ContactEmail] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [IsApproved] bit NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [VerificationCampaigns] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [DepartmentId] int NULL,
        [LocationId] int NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VerificationCampaigns] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [Budgets] (
        [Id] int NOT NULL IDENTITY,
        [FiscalYear] int NOT NULL,
        [DepartmentId] int NOT NULL,
        [Type] int NOT NULL,
        [Allocated] decimal(18,2) NOT NULL,
        [Committed] decimal(18,2) NOT NULL,
        [Spent] decimal(18,2) NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Budgets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Budgets_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeNumber] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [EntraObjectId] nvarchar(450) NULL,
        [DepartmentId] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseRequestLines] (
        [Id] int NOT NULL IDENTITY,
        [PurchaseRequestId] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_PurchaseRequestLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseRequestLines_PurchaseRequests_PurchaseRequestId] FOREIGN KEY ([PurchaseRequestId]) REFERENCES [PurchaseRequests] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseOrders] (
        [Id] int NOT NULL IDENTITY,
        [Number] nvarchar(max) NOT NULL,
        [PurchaseRequestId] int NOT NULL,
        [VendorId] int NOT NULL,
        [Status] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ReceivedDate] datetime2 NULL,
        [InvoiceNumber] nvarchar(max) NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrders_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [VerificationApprovals] (
        [Id] int NOT NULL IDENTITY,
        [CampaignId] int NOT NULL,
        [ApproverRole] nvarchar(max) NOT NULL,
        [ApproverName] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [DecidedUtc] datetime2 NULL,
        [Comment] nvarchar(max) NULL,
        [VerificationCampaignId] int NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VerificationApprovals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VerificationApprovals_VerificationCampaigns_VerificationCampaignId] FOREIGN KEY ([VerificationCampaignId]) REFERENCES [VerificationCampaigns] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [Assets] (
        [Id] int NOT NULL IDENTITY,
        [AssetTag] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Manufacturer] nvarchar(max) NULL,
        [Model] nvarchar(max) NULL,
        [SerialNumber] nvarchar(100) NULL,
        [CategoryId] int NOT NULL,
        [LocationId] int NULL,
        [DepartmentId] int NULL,
        [OwnerEmployeeId] int NULL,
        [CustodianEmployeeId] int NULL,
        [Status] int NOT NULL,
        [Condition] int NOT NULL,
        [Classification] int NOT NULL,
        [VendorId] int NULL,
        [PurchaseDate] datetime2 NULL,
        [PurchaseCost] decimal(18,2) NULL,
        [WarrantyEnd] datetime2 NULL,
        [IntuneDeviceId] nvarchar(max) NULL,
        [IpAddress] nvarchar(45) NULL,
        [MacAddress] nvarchar(17) NULL,
        [LastVerifiedUtc] datetime2 NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Assets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Assets_AssetCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [AssetCategories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Assets_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Assets_Employees_CustodianEmployeeId] FOREIGN KEY ([CustodianEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Assets_Employees_OwnerEmployeeId] FOREIGN KEY ([OwnerEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Assets_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Assets_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [AssetMovements] (
        [Id] int NOT NULL IDENTITY,
        [AssetId] int NOT NULL,
        [Type] int NOT NULL,
        [FromEmployeeId] int NULL,
        [ToEmployeeId] int NULL,
        [FromLocationId] int NULL,
        [ToLocationId] int NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_AssetMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AssetMovements_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [MaintenanceRecords] (
        [Id] int NOT NULL IDENTITY,
        [AssetId] int NOT NULL,
        [Type] int NOT NULL,
        [Status] int NOT NULL,
        [ScheduledDate] datetime2 NOT NULL,
        [CompletedDate] datetime2 NULL,
        [Cost] decimal(18,2) NULL,
        [Description] nvarchar(max) NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_MaintenanceRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MaintenanceRecords_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [VerificationItems] (
        [Id] int NOT NULL IDENTITY,
        [CampaignId] int NOT NULL,
        [AssetId] int NOT NULL,
        [Result] int NOT NULL,
        [Method] int NULL,
        [VerifiedAtUtc] datetime2 NULL,
        [VerifiedBy] nvarchar(max) NULL,
        [ClientId] uniqueidentifier NULL,
        [Latitude] float NULL,
        [Longitude] float NULL,
        [Comment] nvarchar(max) NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VerificationItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VerificationItems_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VerificationItems_VerificationCampaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [VerificationCampaigns] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE TABLE [VerificationEvidence] (
        [Id] int NOT NULL IDENTITY,
        [VerificationItemId] int NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [StoragePath] nvarchar(max) NOT NULL,
        [Sha256] nvarchar(max) NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VerificationEvidence] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VerificationEvidence_VerificationItems_VerificationItemId] FOREIGN KEY ([VerificationItemId]) REFERENCES [VerificationItems] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AssetMovements_AssetId] ON [AssetMovements] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Assets_AssetTag] ON [Assets] ([AssetTag]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_CategoryId] ON [Assets] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_CustodianEmployeeId] ON [Assets] ([CustodianEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_DepartmentId] ON [Assets] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_LocationId] ON [Assets] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_OwnerEmployeeId] ON [Assets] ([OwnerEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_SerialNumber] ON [Assets] ([SerialNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_Status_DepartmentId] ON [Assets] ([Status], [DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_VendorId] ON [Assets] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityName_EntityKey] ON [AuditLogs] ([EntityName], [EntityKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_TimestampUtc] ON [AuditLogs] ([TimestampUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Budgets_DepartmentId] ON [Budgets] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Budgets_FiscalYear_DepartmentId_Type] ON [Budgets] ([FiscalYear], [DepartmentId], [Type]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Departments_Code] ON [Departments] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Departments_ParentId] ON [Departments] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Employees_EmployeeNumber] ON [Employees] ([EmployeeNumber]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_EntraObjectId] ON [Employees] ([EntraObjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRecords_AssetId] ON [MaintenanceRecords] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_VendorId] ON [PurchaseOrders] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseRequestLines_PurchaseRequestId] ON [PurchaseRequestLines] ([PurchaseRequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseRequests_Number] ON [PurchaseRequests] ([Number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VerificationApprovals_VerificationCampaignId] ON [VerificationApprovals] ([VerificationCampaignId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VerificationEvidence_VerificationItemId] ON [VerificationEvidence] ([VerificationItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VerificationItems_AssetId] ON [VerificationItems] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VerificationItems_CampaignId_AssetId] ON [VerificationItems] ([CampaignId], [AssetId]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VerificationItems_ClientId] ON [VerificationItems] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008125137_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261008125137_InitialCreate', N'8.0.8');
END;
GO

COMMIT;
GO

