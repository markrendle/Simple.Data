CREATE TABLE [dbo].[Users]
(
	Id int NOT NULL identity(1,1), 
	Name nvarchar(100) not null,
	[Password] nvarchar(100) not null,
	Age int not null
)
