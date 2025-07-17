CREATE OR ALTER PROCEDURE RegistrarTrasnferencia
    @IdPix UNIQUEIDENTIFIER,
    @Going VARCHAR(255),
    @Coming VARCHAR(255),
    @Amount DECIMAL(18,2)
AS
BEGIN
    IF @Amount < 0
    BEGIN
        RAISERROR('O valor da transferência não pode ser negativo.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO TransactionsTable(Id, Going, Coming, Amount)
        VALUES(@IdPix, @Going, @Coming, @Amount);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErroMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('Erro ao inserir a linha: %s', 16, 1, @ErroMsg);
    END CATCH
END;