CREATE PROCEDURE RealizarTransferencia
    @AccountNumberFrom NVARCHAR(20),
    @AccountNumberTo NVARCHAR(20),
    @Amount DECIMAL(18,2),
    @Description NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Amount <= 0
        BEGIN
            THROW 50003, 'Valor da transferência inválido. Deve ser maior que zero.', 1;
        END

        DECLARE @SaldoOrigem DECIMAL(18,2);

        SELECT @SaldoOrigem = Balance 
        FROM Accounts 
        WHERE AccountNumber = @AccountNumberFrom;

        IF @SaldoOrigem IS NULL
        BEGIN
            THROW 50001, 'Conta de origem não encontrada.', 1;
        END

        IF @SaldoOrigem < @Amount
        BEGIN
            THROW 50002, 'Saldo insuficiente na conta de origem.', 1;
        END

        UPDATE Accounts 
        SET Balance = Balance - @Amount
        WHERE AccountNumber = @AccountNumberFrom;

        UPDATE Accounts
        SET Balance = Balance + @Amount
        WHERE AccountNumber = @AccountNumberTo;

        INSERT INTO Transfers 
            (AccountNumberFrom, AccountNumberTo, Amount, TransferDate, Description)
        VALUES 
            (@AccountNumberFrom, @AccountNumberTo, @Amount, GETDATE(), @Description);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
        THROW 50000, @ErrorMessage, 1;
    END CATCH
END;