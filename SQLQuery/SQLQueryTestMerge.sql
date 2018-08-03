USE InfoDirectory
GO

DECLARE @table AS ErrorCodeTableType;  

/* Add data to the table variable. */  
INSERT INTO @table (code, [text]) Values  (6,'6685'),(4,'4444');

Exec Test4 @table;

Select * from ErrorCode



Select * from ErrorCode WHERE code NOT IN (SELECT code FROM @table)

