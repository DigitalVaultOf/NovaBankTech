CREATE OR ALTER PROCEDURE ProcessPaymentProcedure
    @AccountNumber NVARCHAR(20),
    @Value DECIMAL(18,2)
    AS
BEGIN
    SET NOCOUNT ON;

BEGIN TRY
BEGIN TRANSACTION;

        -- 1. Validações Iniciais
        IF @Value <= 0
BEGIN
            -- Desfaz a transação e lança um erro claro
ROLLBACK TRANSACTION;
THROW 50003, 'Valor de pagamento inválido. O valor deve ser maior que zero.', 1;
            RETURN;
END

        -- 2. Verifica o saldo da conta
        DECLARE @SaldoOrigin DECIMAL(18,2);
SELECT @SaldoOrigin = Balance
FROM Accounts
WHERE AccountNumber = @AccountNumber;

IF @SaldoOrigin IS NULL
BEGIN
ROLLBACK TRANSACTION;
THROW 50001, 'Conta não encontrada para o débito do pagamento.', 1;
            RETURN;
END

        IF @SaldoOrigin < @Value
BEGIN
ROLLBACK TRANSACTION;
THROW 50002, 'Saldo insuficiente para realizar o pagamento.', 1;
            RETURN;
END

        -- 3. Debita o valor do pagamento do saldo da conta
UPDATE Accounts
SET Balance = Balance - @Value
WHERE AccountNumber = @AccountNumber;

-- 4. Insere o registro da movimentação, com o tipo 'Pagamento' fixo
INSERT INTO Moviments (Amount, AccountNumber, MovimentTypeEnum, DateTimeMoviment)
VALUES (@Value, @AccountNumber, 3, GETDATE()); -- Usando o valor 3 para 'Pagamento'

-- 5. Se tudo deu certo, confirma as alterações
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
        -- Se qualquer erro ocorrer, desfaz tudo
IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Relança o erro para que a aplicação saiba que algo deu errado
        THROW;
END CATCH
END;