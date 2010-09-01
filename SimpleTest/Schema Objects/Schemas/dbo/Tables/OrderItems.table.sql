CREATE TABLE [dbo].[OrderItems]
(
	OrderItemId int NOT NULL IDENTITY(1,1), 
	OrderId int not null,
	ItemId int not null,
	Quantity int not null
)
