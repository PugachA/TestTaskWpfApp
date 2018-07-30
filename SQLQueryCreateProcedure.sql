USE InfoDirectory;
GO

--��������� ��� ������ � ������� ErrorCode
CREATE PROCEDURE [dbo].[sp_AddErrorCode]
    @code int,
    @text nvarchar(100)
AS
DECLARE @codeintable INT
SET @codeintable = (SELECT TOP 1 code FROM ErrorCode WHERE code = @code) --TOP 1 ������ ��� �������� ���� � ���������� ����� ��������� ��������

IF @codeintable=@code
begin
	UPDATE ErrorCode
	SET [text]=@text WHERE code=@code
end
ELSE INSERT INTO ErrorCode(code, [text]) VALUES (@code, @text)
GO

--��������� ��� ������ � ������� Category
CREATE PROCEDURE [dbo].[sp_AddCategory]
    @id int,
    @name nvarchar(100),
	@parent int,
	@image nvarchar(100)
AS
DECLARE @idintable INT
SET @idintable = (SELECT TOP 1 id FROM Category WHERE id = @id) --TOP 1 ������ ��� �������� ���� � ���������� ����� ��������� ��������

IF @idintable=@id
begin
	UPDATE Category
	SET [name]=@name, parent=@parent, [image]=@image WHERE id=@id
end
ELSE INSERT INTO Category(id, [name],parent,[image]) VALUES (@id, @name,@parent,@image)
GO