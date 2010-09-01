CREATE TABLE [dbo].[Items]
(
	ItemId int NOT NULL IDENTITY(1,1), 
	Name nvarchar(100) not null,
	Price money not null
)
