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
INSERT (code,[text]) VALUES (Source.code,Source.[text]);

--удаляем значения, если список пришел короче
DELETE ErrorCode WHERE code NOT IN (SELECT code FROM @table);
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
INSERT (id,[name],parent,[image]) VALUES (Source.id,Source.[name],Source.parent,Source.[image]);

--удаляем значения, если список пришел короче
DELETE Category WHERE id NOT IN (SELECT id FROM @table);
GO