USE InfoDirectory;
GO

CREATE TYPE ErrorCodeTableType AS TABLE (
	code int UNIQUE,
	[text] nvarchar(100)
);
GO

CREATE TYPE CategoryTableType AS TABLE (
	id int UNIQUE,
	[name] nvarchar(100),
	parent int,
	[image] nvarchar(100)
);
GO
