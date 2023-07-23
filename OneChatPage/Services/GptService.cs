using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Messaging;
using Humanizer;
using Microsoft.Identity.Client;
using OneChatPage.Extensions;
using OneChatPage.ViewModels;
using System.Text.Json;

namespace OneChatPage.Services
{
    public class GptService
    {
        public async Task<string> GetAnser(EmbViewModel input)
        {
            var endpoint = "https://bulingschoolteam06.openai.azure.com/";
            var client = new OpenAIClient(new Uri(endpoint),
                new AzureKeyCredential("cd380ab618db41448a7a0e69c130f2a3"));
            string model = "gpt-35-t";

            var prompt = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, string.Join(',', input.Refer)),
                    new ChatMessage(ChatRole.User, input.Message)
                }
            };

            Response<ChatCompletions> Response =await client.GetChatCompletionsAsync(model, prompt);
            var result = Response.Value.Choices[0].Message.Content.ToString();

            return result;
        }

        public async Task<float[]> GetEmbedding(string message)
        {
            var endpoint = "https://bulingschoolteam06.openai.azure.com/";
            var client = new OpenAIClient(new Uri(endpoint),
                new AzureKeyCredential("cd380ab618db41448a7a0e69c130f2a3"));
            string model = "emb-ada"; 

            var prompt = new EmbeddingsOptions(message);

            var Embeddings = await client.GetEmbeddingsAsync(model, prompt);
            var result = Embeddings.Value.Data[0].Embedding.ToArray();

            return result;
        }
    }
}
