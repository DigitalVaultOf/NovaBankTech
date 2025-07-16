CREATE OR ALTER PROCEDURE MovimentationsDepositProcedure
    @AccountNumber NVARCHAR(20),
    @Value DECIMAL(18,2),
    @MovimentType NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validações de parâmetros
        IF @AccountNumber IS NULL OR LTRIM(RTRIM(@AccountNumber)) = ''
        BEGIN
            THROW 50002, 'Número da conta é obrigatório.', 1;
        END
        
        IF @MovimentType IS NULL OR LTRIM(RTRIM(@MovimentType)) = ''
        BEGIN
            THROW 50004, 'Tipo de movimentação é obrigatório.', 1;
        END
        
        IF @Value <= 0
        BEGIN
            THROW 50003, 'Valor de depósito inválido. O valor deve ser maior que zero.', 1;
        END
        
        -- Verificar se a conta existe
        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountNumber = @AccountNumber)
        BEGIN
            THROW 50001, 'Conta não encontrada.', 1;
        END
        
        -- Atualizar saldo
        UPDATE Accounts
        SET Balance = Balance + @Value
        WHERE AccountNumber = @AccountNumber;
        
        -- Verificar se o UPDATE foi bem-sucedido
        IF @@ROWCOUNT = 0
        BEGIN
            THROW 50001, 'Falha ao atualizar o saldo da conta.', 1;
        END
        
        -- Inserir movimentação
        INSERT INTO Moviments (Amount, AccountNumber, MovimentTypeEnum, DateTimeMoviment)
        VALUES (@Value, @AccountNumber, @MovimentType, GETDATE());
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        THROW @ErrorSeverity, @ErrorMessage, @ErrorState;
    END CATCH
END;