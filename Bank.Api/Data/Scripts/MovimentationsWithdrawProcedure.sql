CREATE OR ALTER PROCEDURE MovimentationsWithdrawProcedure
    @AccountNumber NVARCHAR(20),
    @Value DECIMAL(18,2),
    @MovimentType NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Value <= 0
        BEGIN
            THROW 50003, 'Valor de saque inválido. O valor deve ser maior que zero.', 1;
        END

        DECLARE @SaldoOrigin DECIMAL(18,2);

        SELECT @SaldoOrigin = Balance 
        FROM Accounts 
        WHERE AccountNumber = @AccountNumber;

        IF @SaldoOrigin IS NULL
        BEGIN
            THROW 50001, 'Conta de origem não encontrada.', 1;
        END

        IF @SaldoOrigin < @Value
        BEGIN
            THROW 50002, 'Saldo insuficiente para saque.', 1;
        END

        UPDATE Accounts
        SET Balance = Balance - @Value
        WHERE AccountNumber = @AccountNumber;

        INSERT INTO Moviments (Amount, AccountNumber, MovimentTypeEnum, DateTimeMoviment)
        VALUES (@Value, @AccountNumber, @MovimentType, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
        THROW 50000, @ErrorMessage, 1;
    END CATCH
END;