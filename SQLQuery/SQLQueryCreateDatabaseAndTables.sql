CREATE DATABASE InfoDirectory;
GO

USE InfoDirectory;

CREATE TABLE ErrorCode
(
	code int,
	[text] nvarchar(100)
)
GO

CREATE TABLE Category
(
	id int,
	[name] nvarchar(100),
	parent int,
	[image] nvarchar(100)
)
GO