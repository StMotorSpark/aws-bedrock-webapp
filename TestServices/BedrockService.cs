namespace TestServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Util;
using Amazon.SSO;
using Amazon.SSOOIDC;

public class BedrockService
{
    public async Task<string> RunPrompt(string prompt)
    {
        string model = "amazon.titan-text-lite-v1";
        AmazonBedrockRuntimeClient client = new(RegionEndpoint.USEast1);

        string payload = new JsonObject()
            {
                { "inputText", prompt },
                { "textGenerationConfig", new JsonObject()
                    {
                        { "maxTokenCount", 512 },
                        { "temperature", 0f },
                        { "topP", 1f }
                    }
                }
            }.ToJsonString();
        string generatedText = "";
        try
        {
            InvokeModelResponse response = await client.InvokeModelAsync(new InvokeModelRequest()
            {
                ModelId = model,
                Body = AWSSDKUtils.GenerateMemoryStreamFromString(payload),
                ContentType = "application/json",
                Accept = "application/json"
            });

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonNode.ParseAsync(response.Body).Result?["results"]?.AsArray();

                return results is null ? "" : string.Join(" ", results.Select(x => x?["outputText"]?.GetValue<string?>()));
            }
            else
            {
                Console.WriteLine("InvokeModelAsync failed with status code " + response.HttpStatusCode);
            }
        }
        catch (AmazonBedrockRuntimeException e)
        {
            Console.WriteLine(e.Message);
        }
        return generatedText;
    }

    public async Task<IList<KnowledgeBaseInfoItem>> RunPromptOnData(string prompt) {
        IList<KnowledgeBaseInfoItem> results = new List<KnowledgeBaseInfoItem>();

        string model = "amazon.titan-text-lite-v1";
        string knowledgeBaseId = "APZB5AG8DX";
        AmazonBedrockAgentRuntimeClient client = new(RegionEndpoint.USEast1);

        string generatedText = "";
        try
        {
            RetrieveResponse response = await client.RetrieveAsync(new RetrieveRequest()
            {
                RetrievalQuery = new KnowledgeBaseQuery()
                {
                    Text = prompt
                },
                KnowledgeBaseId = knowledgeBaseId
            });

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                results = (from KnowledgeBaseRetrievalResult item in response.RetrievalResults
                    select new KnowledgeBaseInfoItem
                    {
                        Content = item.Content.Text,
                        Path = item.Location.S3Location.Uri
                    }).ToList();
                // var results = JsonNode.Parse(resultsDetails).Result?["results"]?.AsArray();

                // return results is null ? "" : string.Join(" ", results.Select(x => x?["outputText"]?.GetValue<string?>()));
            }
            else
            {
                Console.WriteLine("InvokeModelAsync failed with status code " + response.HttpStatusCode);
            }
        }
        catch (AmazonBedrockAgentRuntimeException e)
        {
            Console.WriteLine(e.Message);
        }
        return results;
    }

    public async Task<string> RunRagPrompt(string prompt) {
        // currently this model is not supported. Will need to wait until the amazon titan models
        // can be used with the bedrock agent runtime client
        string model = "amazon.titan-text-lite-v1";
        string knowledgeBaseId = "APZB5AG8DX";

        AmazonBedrockAgentRuntimeClient client = new(RegionEndpoint.USEast1);
        
        string generatedText = "";
        try {
            RetrieveAndGenerateResponse response = await client.RetrieveAndGenerateAsync(new RetrieveAndGenerateRequest() {
                SessionId = Guid.NewGuid().ToString(),
                RetrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration() {
                    Type = "KNOWLEDGE_BASE",
                    KnowledgeBaseConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration() {
                        KnowledgeBaseId = knowledgeBaseId,
                        ModelArn = $"arn:aws:bedrock:us-east-1::foundation-model/{model}"
                    }
                },
                Input = new RetrieveAndGenerateInput() {
                    Text = prompt
                }
            });

            if(response.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                generatedText = response.Output.Text;
            } else {
                Console.WriteLine("RetrieveAndGenerateAsync failed with status code " + response.HttpStatusCode);
            }
        } catch(AmazonBedrockRuntimeException e) {
            Console.WriteLine(e.Message);
        }

        return generatedText;
    }
}

public class KnowledgeBaseInfoItem {
    public string Content { get; set; }
    public string Path { get; set; }
}