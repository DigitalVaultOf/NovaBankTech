using Ai.Api.DTOS;
using Google.Api.Gax.Grpc; // Para o CallSettings
using Google.Cloud.AIPlatform.V1;
// Usamos um alias para resolver a ambiguidade
using ProtoValue = Google.Protobuf.WellKnownTypes.Value;
using ProtoStruct = Google.Protobuf.WellKnownTypes.Struct;

namespace Ai.Api.Services;

public class AiService(IConfiguration configuration) : IAiService
{
    public async Task<ChatbotResponseDto> AskQuestionAsync(AskQuestionDto questionDto)
    {
        // 1. Pega a chave da API do appsettings.json
        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("Chave da API do Gemini não configurada.");
        }

        // 2. Define o endpoint da API para a sua região
        const string endpoint = "us-central1-aiplatform.googleapis.com";

        // 3. Usa o Builder para criar o cliente (sem a chave aqui)
        var client = await new PredictionServiceClientBuilder
        {
            Endpoint = endpoint
        }.BuildAsync();

        // 4. Monta o prompt com o contexto do seu banco
        var prompt = $"""

                                  Você é um assistente virtual para o banco 'NovaBankTech'.
                                  Sua personalidade é prestativa e direta.
                                  Responda APENAS com base nas funcionalidades do banco descritas abaixo.
                                  Se a pergunta não estiver relacionada a estas funcionalidades, responda educadamente que você só pode ajudar com questões sobre o NovaBankTech.

                                  Funcionalidades do NovaBankTech:
                                  - Pagamento de Boletos: O usuário pode pagar boletos na tela de pagamentos. Ele precisa do número do boleto e da senha da conta para confirmar. O sistema verifica o saldo antes de debitar.
                                  - Histórico de Pagamentos: Na tela de pagamentos, o usuário pode listar os boletos pagos e os pendentes.
                                  - Transferências: O usuário pode transferir dinheiro para outras contas do NovaBankTech.
                                  - Saques e Depósitos: Funções disponíveis na tela inicial.
                                  - PIX: O sistema possui funcionalidade de PIX.
                                  - Gestão de Conta: O usuário pode editar seus dados (nome, email) e alterar sua senha na área de 'Configurações'.
                                  - Desativar Conta: O usuário pode desativar sua conta, uma ação que é irreversível.
                                  - Saldo: O saldo atual é sempre exibido no topo da página inicial.
                                  - Exportar Histórico: O usuário pode exportar seu histórico de movimentações.

                                  Com base nisso, responda à seguinte pergunta do usuário: '{questionDto.Question}'
                              
                      """;

        // 5. Monta a requisição para o Gemini (usando os aliases para resolver a ambiguidade)
        var request = new PredictRequest
        {
            Endpoint =
                "projects/gothic-surf-430919-g8/locations/us-central1/publishers/google/models/gemini-1.5-pro-preview-0409",
            Instances =
            {
                ProtoValue.ForStruct(new ProtoStruct
                {
                    Fields = { { "content", ProtoValue.ForString(prompt) } }
                })
            }
        };

        // 6. Cria as opções de chamada, adicionando a chave da API no cabeçalho
        var callSettings = CallSettings.FromHeader("X-Goog-Api-Key", apiKey);

        // 7. Envia a requisição e recebe a resposta (passando as opções de chamada)
        var response = await client.PredictAsync(request, callSettings);
        var predictionResult = response.Predictions.FirstOrDefault();

        // 8. VERIFICAÇÃO DE NULO (corrige o alerta do Rider)
        if (predictionResult == null)
        {
            throw new Exception("A API do Gemini não retornou uma previsão válida.");
        }

        // 9. Extrai a resposta de texto da estrutura complexa do Gemini
        var answer = predictionResult.StructValue.Fields["candidates"].ListValue.Values[0].StructValue.Fields["content"]
            .StructValue.Fields["parts"].ListValue.Values[0].StructValue.Fields["text"].StringValue;

        return new ChatbotResponseDto(answer);
    }
}