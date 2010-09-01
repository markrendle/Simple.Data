CREATE TABLE [dbo].[Customers]
(
	CustomerId int NOT NULL IDENTITY(1,1), 
	Name nvarchar(100) not null,
	[Address] nvarchar(200)
)
