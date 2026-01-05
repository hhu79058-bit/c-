USE WaiMaiSystem;
GO

-- 检查是否存在管理员账号，如果不存在则插入默认管理员
IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserType = 2)
BEGIN
    INSERT INTO [User] (UserName, Password, PhoneNumber, Address, UserType)
    VALUES ('admin', '123456', '13800000000', 'Admin Center', 2);
END
GO
