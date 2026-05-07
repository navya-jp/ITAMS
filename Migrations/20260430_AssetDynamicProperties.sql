-- Dynamic Asset Properties Feature
-- Creates AssetPropertiesMaster and AssetPropertyValues tables

IF OBJECT_ID('[AssetPropertiesMaster]', 'U') IS NULL
CREATE TABLE [AssetPropertiesMaster] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PropertyName] nvarchar(100) NOT NULL,
    [ApplicableSubtype] nvarchar(100) NULL,  -- NULL = global/all subtypes
    [DataType] nvarchar(20) NOT NULL DEFAULT ('Text'), -- Text, Number, Boolean
    [IsActive] bit NOT NULL DEFAULT (1),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [CreatedBy] int NOT NULL DEFAULT (1),
    CONSTRAINT [PK_AssetPropertiesMaster] PRIMARY KEY ([Id])
);
GO

IF OBJECT_ID('[AssetPropertyValues]', 'U') IS NULL
CREATE TABLE [AssetPropertyValues] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetId] int NOT NULL,
    [PropertyId] int NOT NULL,
    [PropertyValue] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_AssetPropertyValues] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AssetPropertyValues_Assets] FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AssetPropertyValues_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [AssetPropertiesMaster]([Id])
);
GO

-- Seed common properties per subtype
SET IDENTITY_INSERT [AssetPropertiesMaster] ON;

INSERT INTO [AssetPropertiesMaster] ([Id],[PropertyName],[ApplicableSubtype],[DataType],[IsActive],[CreatedBy]) VALUES
(1,'Color Printing','Printer','Boolean',1,1),
(2,'Print Limit','Printer','Number',1,1),
(3,'Cartridge Type','Printer','Text',1,1),
(4,'Duplex Support','Printer','Boolean',1,1),
(5,'Resolution','Camera','Text',1,1),
(6,'Night Vision','Camera','Boolean',1,1),
(7,'Storage Capacity','Camera','Text',1,1),
(8,'Camera Type','Camera','Text',1,1),
(9,'RAM','Laptop','Text',1,1),
(10,'Storage','Laptop','Text',1,1),
(11,'Processor','Laptop','Text',1,1),
(12,'OS Version','Laptop','Text',1,1),
(13,'RAM','Desktop','Text',1,1),
(14,'Storage','Desktop','Text',1,1),
(15,'Processor','Desktop','Text',1,1),
(16,'OS Version','Desktop','Text',1,1),
(17,'Capacity (KVA)','UPS','Number',1,1),
(18,'Battery Backup (mins)','UPS','Number',1,1),
(19,'Port Count','Switch','Number',1,1),
(20,'POE Support','Switch','Boolean',1,1),
(21,'Managed/Unmanaged','Switch','Text',1,1),
(22,'RAM','Server','Text',1,1),
(23,'Storage','Server','Text',1,1),
(24,'Processor','Server','Text',1,1),
(25,'Screen Size','Monitor','Text',1,1),
(26,'Resolution','Monitor','Text',1,1),
(27,'Panel Type','Monitor','Text',1,1),
(28,'Bandwidth','Router','Text',1,1),
(29,'WiFi Standard','Router','Text',1,1),
(30,'Throughput','Firewall','Text',1,1);

SET IDENTITY_INSERT [AssetPropertiesMaster] OFF;
GO
