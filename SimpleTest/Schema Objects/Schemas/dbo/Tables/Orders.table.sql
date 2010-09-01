CREATE TABLE [dbo].[Orders]
(
	OrderId int NOT NULL IDENTITY(1,1), 
	OrderDate datetime not null,
	CustomerId int not null
)
