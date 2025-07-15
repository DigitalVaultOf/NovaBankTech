CREATE PROCEDURE CriarChave
    @IdPix UNIQUEIDENTIFIER,
    @Name VARCHAR(255),
    @PixKey VARCHAR(255),
    @Bank VARCHAR(255),
    @AccountNumber VARCHAR(255)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Pix (IdPix,NameUser, PixKey, Bank, AccountNumber)
        VALUES(@IdPix,@Name, @PixKey, @Bank, @AccountNumber );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErroMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('Erro ao inserir a linha: %s', 16, 1, @ErroMsg);
    END CATCH
END;