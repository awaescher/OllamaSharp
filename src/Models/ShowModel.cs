using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models
{
	/// <summary>
	/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#show-model-information
	/// </summary>
	public class ShowModelRequest
	{
		/// <summary>
		/// The name of the model to show
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }
	}

	public class ShowModelResponse
	{
		[JsonPropertyName("license")] 
		public string? License { get; set; } 

		[JsonPropertyName("modelfile")] 
		public string? Modelfile { get; set; } 

		[JsonPropertyName("parameters")] 
		public string? Parameters { get; set; }

		[JsonPropertyName("template")]
		public string? Template { get; set; } 

		[JsonPropertyName("system")]
		public string? System { get; set; } 

		[JsonPropertyName("details")]
		public ShowModelResponseDetails Details { get; set; } = null!;
	}
	
	
	public class ShowModelResponseDetails
	{
		[JsonPropertyName("parent_model")]
		public string? ParentModel { get; set; }

		[JsonPropertyName("format")]
		public string Format { get; set; }  = null!;

		[JsonPropertyName("family")]
		public string Family { get; set; }  = null!;

		[JsonPropertyName("families")]
		public List<string>? Families { get; set; } 

		[JsonPropertyName("parameter_size")]
		public string ParameterSize { get; set; } = null!;

		[JsonPropertyName("quantization_level")]
		public string QuantizationLevel { get; set; } = null!;
	}
	
}