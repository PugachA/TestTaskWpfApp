USE InfoDirectory
GO

CREATE PROCEDURE sp_AddErrorCodes
@table ErrorCodeTableType READONLY   
AS

MERGE ErrorCode AS Target
USING @table AS Source
ON Source.code=Target.code

WHEN MATCHED THEN UPDATE SET Target.[text]=Source.[text]

WHEN NOT MATCHED THEN
INSERT (code,[text]) VALUES (Source.code,Source.[text])

WHEN NOT MATCHED BY Source THEN
DELETE;

GO

CREATE PROCEDURE sp_AddCategories
@table CategoryTableType READONLY   
AS

MERGE Category AS Target
USING @table AS Source
ON Source.id=Target.id

WHEN MATCHED THEN
 UPDATE SET Target.[name]=Source.[name], Target.parent=Source.parent, Target.[image]=Source.[image]

WHEN NOT MATCHED THEN
INSERT (id,[name],parent,[image]) VALUES (Source.id,Source.[name],Source.parent,Source.[image])

WHEN NOT MATCHED BY Source THEN
DELETE;

GO