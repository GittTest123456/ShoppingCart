CREATE DATABASE ShoppingCart1
GO

USE ShoppingCart1
GO
CREATE TABLE [dbo].[User] (
    [Username] VARCHAR (100)    NOT NULL,
    [Password] VARCHAR (100)    NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Username] ASC))
GO

CREATE TABLE [dbo].[Session] (
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [Username]  VARCHAR (100) NOT NULL,
    [Timestamp] BIGINT           NOT NULL,
    CONSTRAINT [PK_Session] PRIMARY KEY CLUSTERED ([SessionId] ASC),
    CONSTRAINT [FK_1] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username]))
GO

CREATE TABLE  [dbo].[Product](
    [ProductId] INT NOT NULL,
    [Name] VARCHAR(100) NOT NULL,
    [Price] INT NOT NULL,
    [Description] VARCHAR(100) NOT NULL,
    [AvgRating] INT,
    CONSTRAINT Con_AvgRating CHECK(AvgRating BETWEEN 1 and 5),
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([ProductId] ASC))
GO

CREATE TABLE [dbo].[Cart](
    [Username] VARCHAR (100) NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    PRIMARY KEY ([Username],[ProductId]),
    CONSTRAINT [FK_8] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]))

GO

CREATE TABLE [dbo].[OrderList](
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [Username] VARCHAR (100) NOT NULL,
    [PurchaseDate] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_OrderList] PRIMARY KEY CLUSTERED ([OrderId] ASC),
    CONSTRAINT [FK_2] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username]))


GO

CREATE TABLE [dbo].[OrderDetails](
    [ActivationCode] UNIQUEIDENTIFIER NOT NULL,
    [OrderId]  UNIQUEIDENTIFIER NOT NULL,
    [ProductId] INT NOT NULL,
    CONSTRAINT [PK_OrderDetails] PRIMARY KEY CLUSTERED ([ActivationCode] ASC),
    CONSTRAINT [FK_3] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[OrderList] ([OrderId]),
    CONSTRAINT [FK_4] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]))


GO

CREATE TABLE [dbo].[PersonalRating](
    [PurchaseDate]  VARCHAR(100) NOT NULL,
    [ProductId] INT NOT NULL,
    [Rating] INT,
    [Username] VARCHAR (100) NOT NULL,
    PRIMARY KEY ([PurchaseDate],[ProductId],[Username]),
    CONSTRAINT [FK_5] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]))

INSERT INTO dbo.[User] (Username,Password) VALUES
    (N'john',N'john'),
	(N'mary',N'mary'),
	(N'peter',N'peter')


INSERT INTO dbo.[Product] (ProductId, Name, Price, Description, AvgRating) VALUES
    (01,'.NET Charts',99,'Brings powerful charting capabilities to your .NET applications.', 5),
    (02,'.NET Paypal',69,'Integrate your .NET apps with PayPal the easy way!', null),
    (03,'.NET ML',299,'Supercharged .NET machine learning libraries',4),
    (04,'.NET Analytics',299,'Performs data mining and analytics easily in .NET',3),
    (05,'.NET Logger',49,'Logs and aggregates events easily in your .NET apps',2),
    (06,'.NET Numerics',199,'Powerful numerical methods for your .NET simulations',null)