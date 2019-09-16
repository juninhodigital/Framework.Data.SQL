USE DB_SYSADV
GO
 
EXEC SP_DROP_IF_EXISTS 'SP_USER_I'
GO

CREATE PROCEDURE SP_USER_I
(
	@P_ID		INT = NULL OUTPUT,
	@P_Name		varchar(80),
	@P_Nickname	varchar(50),
	@P_RG		varchar(50),
	@P_CPF		varchar(50),
	@P_Enabled	bit,	
	@P_TVP_ADDRESS TVP_ADDRESS READONLY
 )
 AS
 BEGIN
 
	IF EXISTS
	(
		SELECT 1 FROM TB_USER WITH(NOLOCK)
		WHERE CPF = @P_CPF
	)
	BEGIN
		RAISERROR ('There is already a user with this CPF.Please, try another one.', 16, 1);
	END
	ELSE
	BEGIN
	
		BEGIN TRY

			BEGIN TRANSACTION

			INSERT TB_USER
			(
				Name,
				Nickname,
				RG,
				CPF,
				Enabled
			)
			VALUES
			(
				@P_Name,
				@P_Nickname,
				@P_RG,
				@P_CPF,
				@P_Enabled
			)

			SET @P_ID = @@IDENTITY

			-- Insert the new ones
			INSERT TB_ADDRESS
			SELECT 
				Address,
				@P_ID,
				Enabled
			FROM @P_TVP_ADDRESS

			COMMIT TRANSACTION

		END TRY
		BEGIN CATCH

			IF(@@TRANCOUNT !=0)
			BEGIN
				ROLLBACK TRANSACTION
			END
			
		END CATCH

	END

END
GO
 
GRANT EXECUTE ON SP_USER_I TO PUBLIC
GO