USE master;
GO

IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'WaiMaiSystem')
BEGIN
    CREATE DATABASE WaiMaiSystem;
END
GO

USE WaiMaiSystem;
GO

-- =============================================
-- 1. 用户表 [User]
-- =============================================
IF OBJECT_ID('[User]', 'U') IS NULL
BEGIN
    CREATE TABLE [User] (
        UserID INT IDENTITY(1,1) PRIMARY KEY,
        UserName NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(100) NOT NULL, -- 明文存储（课程设计演示用）
        PhoneNumber NVARCHAR(20) NOT NULL DEFAULT '',
        Address NVARCHAR(200) NOT NULL DEFAULT '',
        UserType INT NOT NULL DEFAULT 0 -- 0:顾客, 1:商家, 2:管理员
    );
END
GO

-- =============================================
-- 2. 商家表 Merchant
-- =============================================
IF OBJECT_ID('Merchant', 'U') IS NULL
BEGIN
    CREATE TABLE Merchant (
        MerchantID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        ShopName NVARCHAR(100) NOT NULL,
        ShopAddress NVARCHAR(200) NOT NULL DEFAULT '',
        ContactPhone NVARCHAR(20) NOT NULL DEFAULT '',
        ShopIntro NVARCHAR(500) NULL,
        LogoUrl NVARCHAR(255) NULL,
        FOREIGN KEY (UserID) REFERENCES [User](UserID)
    );
END
GO

-- =============================================
-- 3. 分类表 Category
-- =============================================
IF OBJECT_ID('Category', 'U') IS NULL
BEGIN
    CREATE TABLE Category (
        CategoryID INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL
    );
END
GO

-- =============================================
-- 4. 菜品表 Food (对应代码中的 Product)
-- =============================================
IF OBJECT_ID('Food', 'U') IS NULL
BEGIN
    CREATE TABLE Food (
        FoodID INT IDENTITY(1,1) PRIMARY KEY,
        MerchantID INT NOT NULL,
        CategoryID INT NULL,
        FoodName NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsAvailable BIT NOT NULL DEFAULT 1,
        ImageUrl NVARCHAR(255) NULL,
        FOREIGN KEY (MerchantID) REFERENCES Merchant(MerchantID),
        FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
    );
END
GO

-- =============================================
-- 5. 订单表 [Order]
-- =============================================
IF OBJECT_ID('[Order]', 'U') IS NULL
BEGIN
    CREATE TABLE [Order] (
        OrderID INT IDENTITY(1,1) PRIMARY KEY,
        OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
        UserID INT NOT NULL,
        MerchantID INT NOT NULL,
        OrderTime DATETIME NOT NULL DEFAULT GETDATE(),
        OrderAmount DECIMAL(18,2) NOT NULL,
        OrderStatus INT NOT NULL DEFAULT 0, -- 0:待接单, 1:已接单, 2:配送中, 3:已完成, 4:已取消
        PayStatus INT NOT NULL DEFAULT 0,   -- 0:待支付, 1:已支付, 2:支付失败
        DeliveryAddress NVARCHAR(200) NOT NULL DEFAULT '',
        FOREIGN KEY (UserID) REFERENCES [User](UserID),
        FOREIGN KEY (MerchantID) REFERENCES Merchant(MerchantID)
    );
END
GO

-- =============================================
-- 6. 订单明细表 OrderDetail (对应代码中的 OrderItem)
-- =============================================
IF OBJECT_ID('OrderDetail', 'U') IS NULL
BEGIN
    CREATE TABLE OrderDetail (
        OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID INT NOT NULL,
        FoodID INT NOT NULL,
        Quantity INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL, -- 记录下单时的单价
        FOREIGN KEY (OrderID) REFERENCES [Order](OrderID),
        FOREIGN KEY (FoodID) REFERENCES Food(FoodID)
    );
END
GO

-- =============================================
-- 7. 支付记录表 Payment
-- =============================================
IF OBJECT_ID('Payment', 'U') IS NULL
BEGIN
    CREATE TABLE Payment (
        PaymentID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID INT NOT NULL,
        PaymentAmount DECIMAL(18,2) NOT NULL,
        PaymentStatus INT NOT NULL DEFAULT 0, -- 0:待支付, 1:已完成, 2:失败
        PaymentTime DATETIME NULL,
        PaymentMethod NVARCHAR(50) NULL,
        FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
    );
END
GO

-- =============================================
-- 8. 地址表 Address
-- =============================================
IF OBJECT_ID('Address', 'U') IS NULL
BEGIN
    CREATE TABLE Address (
        AddressID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        RecipientName NVARCHAR(50) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        FullAddress NVARCHAR(200) NOT NULL,
        IsDefault BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID) REFERENCES [User](UserID)
    );
    -- 索引
    CREATE INDEX IX_Address_User ON Address(UserID);
END
GO

-- =============================================
-- 9. 订单日志表 OrderLog
-- =============================================
IF OBJECT_ID('OrderLog', 'U') IS NULL
BEGIN
    CREATE TABLE OrderLog (
        OrderLogID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID INT NOT NULL,
        FromStatus INT NOT NULL,
        ToStatus INT NOT NULL,
        ChangedAt DATETIME NOT NULL DEFAULT GETDATE(),
        Remark NVARCHAR(200),
        FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
    );
    -- 索引
    CREATE INDEX IX_OrderLog_Order ON OrderLog(OrderID, ChangedAt);
END
GO


-- =============================================
-- 10. 初始化基础数据 (可选)
-- =============================================

-- 插入默认管理员 (admin / 123456)
IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserName = 'admin')
BEGIN
    INSERT INTO [User] (UserName, Password, PhoneNumber, Address, UserType)
    VALUES ('admin', '123456', '13800000000', 'Admin Center', 2);
END

-- 插入默认分类
IF NOT EXISTS (SELECT 1 FROM Category)
BEGIN
    INSERT INTO Category (CategoryName) VALUES ('热销推荐'), ('主食套餐'), ('小吃炸物'), ('饮品甜点');
END

-- 插入一个测试商家 (merchant / 123456)
IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserName = 'merchant')
BEGIN
    INSERT INTO [User] (UserName, Password, PhoneNumber, Address, UserType)
    VALUES ('merchant', '123456', '13911112222', 'Food Street', 1);

    DECLARE @MerchantUserID INT = SCOPE_IDENTITY();

    INSERT INTO Merchant (UserID, ShopName, ShopAddress, ContactPhone, ShopIntro)
    VALUES (@MerchantUserID, '演示商家', '美食街1号', '010-12345678', '主营各种美味快餐，欢迎品尝！');
END

-- 插入一个测试顾客 (user / 123456)
IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserName = 'user')
BEGIN
    INSERT INTO [User] (UserName, Password, PhoneNumber, Address, UserType)
    VALUES ('user', '123456', '13788889999', 'Dormitory A-101', 0);
END
GO
