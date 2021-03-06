USE OTC_PLATFORM
GO

IF EXISTS(SELECT NAME FROM sys.objects WHERE name = 'SP_CREATE_MODEL_S' AND type='P')
BEGIN
	DROP PROCEDURE SP_CREATE_MODEL_S
END
GO

CREATE PROCEDURE SP_CREATE_MODEL_S
(	
	@TABLE VARCHAR(80)
)
AS
BEGIN

	DECLARE @NAME VARCHAR(80) = 'SP' + SUBSTRING(@TABLE,3, LEN(@TABLE)) + '_S'
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @BR CHAR(2) = CHAR(13)
	DECLARE @TAB CHAR(2) = CHAR(9)
	
	SET NOCOUNT ON

	CREATE TABLE #TB_TEMP
	(
		ID							int IDENTITY(1,1) PRIMARY KEY,
		COLUMN_NAME					sysname,
		DATA_TYPE					nvarchar(256),
		CHARACTER_MAXIMUM_LENGTH	int,
		NUMERIC_PRECISION			tinyint,
		NUMERIC_SCALE				int,
		IS_NULLABLE					varchar(3)
	)

	INSERT INTO #TB_TEMP
	SELECT		
		COLUMN_NAME,
		DATA_TYPE,
		CHARACTER_MAXIMUM_LENGTH,
		NUMERIC_PRECISION,
		NUMERIC_SCALE,
		IS_NULLABLE	
	FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= @TABLE	

	-- Set the database
	SET @SQL = ' USE ' + DB_NAME() + @BR
	SET @SQL+= 'GO' + @BR + @BR
	
	-- Delete the previous procedure version
	SET @SQL+= 'IF EXISTS(SELECT NAME FROM sys.objects WHERE name = ''' + @NAME + ''' AND type=''P'')' + @BR
	SET @SQL+= 'BEGIN' + @BR
		SET @SQL+= @TAB + 'DROP PROCEDURE '+ @NAME +'' + @BR
	SET @SQL+= 'END' + @BR
	SET @SQL+= 'GO' + @BR + @BR
	
	-- Create procedure statement
	SET @SQL+='CREATE PROCEDURE '+ @NAME +''  + @BR
	SET @SQL+='AS' + @BR
	SET @SQL+='BEGIN' + @BR + @BR

	SET @SQL+= @TAB + 'SELECT'  + @BR

	WHILE(1=1)
	BEGIN

		DECLARE @ID INT = 0;

		SELECT @ID = (SELECT MIN(ID) FROM #TB_TEMP)		

		IF(@ID=NULL OR @ID IS NULL)
		BEGIN
			BREAK
		END

		DECLARE @LAST_ROW BIT =1

		IF EXISTS(SELECT ID FROM #TB_TEMP WHERE ID> @ID)
		BEGIN
			SET @LAST_ROW=0
		END

		DECLARE @COLUMN_NAME				sysname
		DECLARE @DATA_TYPE					nvarchar(256)
		DECLARE @CHARACTER_MAXIMUM_LENGTH	int
		DECLARE @NUMERIC_PRECISION			tinyint
		DECLARE @NUMERIC_SCALE				int
		DECLARE @IS_NULLABLE				varchar(3)

		SELECT 
			@COLUMN_NAME = COLUMN_NAME,
			@DATA_TYPE   = DATA_TYPE
		FROM #TB_TEMP WHERE ID = @ID

		IF (@LAST_ROW=0)
		BEGIN
			SET @SQL+= @TAB + @TAB + @COLUMN_NAME + ',' + @BR 
		END
		ELSE
		BEGIN
			SET @SQL+= @TAB + @TAB + @COLUMN_NAME + @BR 
		END
		DELETE FROM #TB_TEMP WHERE ID = @ID
	END

	SET @SQL+=@TAB +'FROM '+ @TABLE +' WITH(NOLOCK)'  + @BR + @BR

	SET @SQL+='END' + @BR
	SET @SQL+='GO' + @BR + @BR

	-- Grant execute statement
	SET @SQL+='GRANT EXECUTE ON '+ @NAME +' TO PUBLIC' + @BR
	SET @SQL+='GO' + @BR

	PRINT @SQL

	DROP TABLE #TB_TEMP
	   	     
	END
	GO

GRANT EXECUTE ON SP_CREATE_MODEL_S TO PUBLIC
GO