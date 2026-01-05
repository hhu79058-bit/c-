USE WaiMaiSystem;
GO

-- Food 表新增 ImageUrl
IF COL_LENGTH('Food', 'ImageUrl') IS NULL
BEGIN
    ALTER TABLE Food
    ADD ImageUrl VARCHAR(500) NULL
END
GO

-- Merchant 表新增 LogoUrl
IF COL_LENGTH('Merchant', 'LogoUrl') IS NULL
BEGIN
    ALTER TABLE Merchant
    ADD LogoUrl VARCHAR(500) NULL
END
GO
