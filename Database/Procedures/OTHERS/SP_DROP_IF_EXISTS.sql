USE DB_SYSADV
GO

IF EXISTS(SELECT NAME FROM sys.objects WHERE name = 'SP_DROP_IF_EXISTS' AND type='P')
BEGIN
	DROP PROCEDURE SP_DROP_IF_EXISTS
END
GO

CREATE PROCEDURE SP_DROP_IF_EXISTS
(
	@Name	VARCHAR(255)
)
AS
BEGIN

	DECLARE @TYPE CHAR(2) = (SELECT TYPE FROM SYS.objects WHERE NAME = @Name)

	SET @TYPE = REPLACE(@TYPE, ' ', '')

	IF(LTRIM(RTRIM(@TYPE)) IN('U','P'))
	BEGIN
		
		DECLARE @CMD VARCHAR(255) =''

		IF(@TYPE = 'U')
		BEGIN
			IF EXISTS(SELECT NAME FROM sys.objects WHERE name=@Name AND type=@TYPE)
			BEGIN
				SET @CMD += 'DROP TABLE '+ @Name
			END
		END
		
		IF(@TYPE = 'P')
		BEGIN
			IF EXISTS(SELECT NAME FROM sys.objects WHERE name=@Name AND type=@TYPE)
			BEGIN
				SET @CMD += 'DROP PROCEDURE '+ @Name
			END
		END

		-- See the output
		PRINT @CMD

		-- Execute the t-sql statement
		EXEC(@CMD)
		
	END
	ELSE
	BEGIN

		IF(@TYPE IS NULL)
		BEGIN
			PRINT 'The current object does not exist in the current database'
		END
		ELSE
		BEGIN
			PRINT 'SQL DBType not allowed at the moment. Type: ' + @TYPE
		END
	END

END
GO

GRANT EXECUTE ON SP_DROP_IF_EXISTS TO PUBLIC
GO