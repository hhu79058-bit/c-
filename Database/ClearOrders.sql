USE WaiMaiSystem;
GO

-- 1. 清除订单日志
DELETE FROM OrderLog;
DBCC CHECKIDENT ('OrderLog', RESEED, 0);

-- 2. 清除支付记录
DELETE FROM Payment;
DBCC CHECKIDENT ('Payment', RESEED, 0);

-- 3. 清除订单明细
DELETE FROM OrderDetail;
DBCC CHECKIDENT ('OrderDetail', RESEED, 0);

-- 4. 清除订单主表
DELETE FROM [Order];
DBCC CHECKIDENT ('[Order]', RESEED, 0);

GO
