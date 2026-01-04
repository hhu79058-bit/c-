USE WaiMaiSystem
GO

-- 新增分类表
IF OBJECT_ID('Category', 'U') IS NULL
BEGIN
    CREATE TABLE Category (
        CategoryID INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName VARCHAR(100) NOT NULL
    )
END
GO

-- 新增地址表
IF OBJECT_ID('Address', 'U') IS NULL
BEGIN
    CREATE TABLE Address (
        AddressID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        RecipientName VARCHAR(50) NOT NULL,
        PhoneNumber VARCHAR(20) NOT NULL,
        FullAddress VARCHAR(200) NOT NULL,
        IsDefault BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID) REFERENCES [User](UserID)
    )
END
GO

-- 新增订单日志表
IF OBJECT_ID('OrderLog', 'U') IS NULL
BEGIN
    CREATE TABLE OrderLog (
        OrderLogID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID INT NOT NULL,
        FromStatus INT NOT NULL,
        ToStatus INT NOT NULL,
        ChangedAt DATETIME NOT NULL DEFAULT GETDATE(),
        Remark VARCHAR(200),
        FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
    )
END
GO

-- Food 表新增 CategoryID
IF COL_LENGTH('Food', 'CategoryID') IS NULL
BEGIN
    ALTER TABLE Food
    ADD CategoryID INT NULL
END
GO

-- Food.CategoryID 外键
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Food_Category'
)
BEGIN
    IF OBJECT_ID('Category', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE Food
        ADD CONSTRAINT FK_Food_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
    END
END
GO

-- 索引补充
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_Address_User' AND object_id = OBJECT_ID('Address')
)
BEGIN
    CREATE INDEX IX_Address_User ON Address(UserID)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderLog_Order' AND object_id = OBJECT_ID('OrderLog')
)
BEGIN
    CREATE INDEX IX_OrderLog_Order ON OrderLog(OrderID, ChangedAt)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_Food_Category' AND object_id = OBJECT_ID('Food')
)
BEGIN
    CREATE INDEX IX_Food_Category ON Food(CategoryID)
END
GO
