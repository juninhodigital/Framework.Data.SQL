USE DB_SYSADV
GO
  
EXEC SP_DROP_IF_EXISTS 'SP_USER_U'
GO

CREATE PROCEDURE SP_USER_U
(
	@P_ID INT,
	@P_Name		varchar(80),
	@P_Nickname	varchar(50),
	@P_RG		varchar(50),
	@P_CPF		varchar(50),
	@P_Enabled	bit,	
	@P_TVP_ADDRESS TVP_ADDRESS READONLY
 )
 AS
 BEGIN
 
	BEGIN TRY

		BEGIN TRANSACTION

		SET @P_Name = ' UPDATED AT ' + CONVERT(VARCHAR, GETDATE(),131)
		
		UPDATE TB_USER SET
			Name		= Name + @P_Name
			--Nickname	=  @P_Nickname,
			--RG		= @P_RG,
			--CP		= @P_CPF,			
			--Enabled	= @P_Enabled
		WHERE ID = @P_ID
		
		-- Delete the previous address
		DELETE FROM TB_ADDRESS WHERE USERCODE = @P_ID
	
		-- Insert the new ones
		INSERT TB_ADDRESS
		SELECT 
			Address,
			UserCode,
			Enabled
		FROM @P_TVP_ADDRESS			

		COMMIT TRANSACTION

	END TRY
	BEGIN CATCH
		
		IF(@@TRANCOUNT>0)
		BEGIN
			ROLLBACK TRANSACTION
			SELECT ERROR_MESSAGE()
		END

	END CATCH

 END
 GO
 
 GRANT EXECUTE ON SP_USER_U TO PUBLIC
 GO