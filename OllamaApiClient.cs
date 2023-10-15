using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// Ähnliche Klassen für andere Anfragen...

public class ApiResponse<T>
{
    public T Data { get; set; }
    // Weitere mögliche Eigenschaften, z.B. Statuscode, Fehlermeldungen, etc.
}

public class OllamaApiClient
{
    private readonly HttpClient _client;

    public OllamaApiClient(string baseAddress)
    {
        _client = new HttpClient() { BaseAddress = new Uri(baseAddress) };
    }

    private async Task<ApiResponse<string>> PostAsync<TRequest>(string endpoint, TRequest request)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        return new ApiResponse<string> { Data = responseBody };
    }

    private async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        return new ApiResponse<TResponse>
        {
            Data = JsonSerializer.Deserialize<TResponse>(responseBody)
        };
    }

    private async Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string endpoint)
    {
        var response = await _client.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return new ApiResponse<TResponse>
        {
            Data = JsonSerializer.Deserialize<TResponse>(responseBody)
        };
    }

    private async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return new ApiResponse<TResponse>
        {
            Data = JsonSerializer.Deserialize<TResponse>(responseBody)
        };
    }

    public async Task<ApiResponse<string>> GenerateAsync(GenerateRequest request)
    {
        return await PostAsync<GenerateRequest>("/api/generate", request);
    }

    public async Task<ApiResponse<TResponse>> CreateModelAsync<TResponse>(CreateRequest request)
    {
        return await PostAsync<CreateRequest, TResponse>("/api/create", request);
    }

    // Hier fügen Sie ähnliche Methoden für andere Endpunkte hinzu...

    public async Task<ApiResponse<TResponse>> DeleteModelAsync<TResponse>(string modelName)
    {
        return await DeleteAsync<TResponse>($"/api/delete?model={modelName}");
    }

    public async Task<ApiResponse<List<TResponse>>> ListLocalModelsAsync<TResponse>()
    {
        return await GetAsync<List<TResponse>>("/api/tags");
    }


    public async Task<ApiResponse<TResponse>> ShowModelAsync<TResponse>(ShowRequest request)
    {
        return await PostAsync<ShowRequest, TResponse>("/api/show", request);
    }

    public async Task<ApiResponse<TResponse>> CopyModelAsync<TResponse>(CopyRequest request)
    {
        return await PostAsync<CopyRequest, TResponse>("/api/copy", request);
    }

    public async Task<ApiResponse<TResponse>> PullModelAsync<TResponse>(PullRequest request)
    {
        return await PostAsync<PullRequest, TResponse>("/api/pull", request);
    }

    public async Task<ApiResponse<TResponse>> PushModelAsync<TResponse>(PushRequest request)
    {
        return await PostAsync<PushRequest, TResponse>("/api/push", request);
    }

    public async Task<ApiResponse<TResponse>> GenerateEmbeddingsAsync<TResponse>(EmbeddingsRequest request)
    {
        return await PostAsync<EmbeddingsRequest, TResponse>("/api/embeddings", request);
    }


    public async Task GenerateAsync(string prompt, string model, IResponseStreamer streamer)
    {
        var generateRequest = new GenerateRequest
        {
            Prompt = prompt,
            Model = model
        };

        using (HttpContent httpContent = new StringContent(JsonSerializer.Serialize(generateRequest), Encoding.UTF8, "application/json"))
        {
            HttpResponseMessage response = await _client.PostAsync("/api/generate", httpContent);

            await ProcessStreamedResponseAsync(response, streamer);
        }
    }

    public interface IResponseStreamer
    {
        void Stream(string response);
    }

    private async Task ProcessStreamedResponseAsync(HttpResponseMessage response, IResponseStreamer streamer)
    {
        if (response.IsSuccessStatusCode)
        {
            using (Stream stream = await response.Content.ReadAsStreamAsync())
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    StreamedResponse streamedResponse = JsonSerializer.Deserialize<StreamedResponse>(line);

                    streamer.Stream(streamedResponse?.Response);
                }
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

    public class StreamedResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
