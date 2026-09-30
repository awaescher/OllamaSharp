#pragma warning disable CS0618 // Obsolete members used in tests
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

using System.Text.Json;

using NUnit.Framework;

using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

using Shouldly;

namespace Tests;

/// <summary>
/// Tests for serialization and deserialization of new and updated model properties.
/// </summary>
public class ModelSerializationTests
{
	/// <summary>
	/// Tests for GenerateRequest serialization.
	/// </summary>
	public class GenerateRequestTests : ModelSerializationTests
	{
		[Test]
		public void Serializes_Think_Property_As_Bool()
		{
			var request = new GenerateRequest { Model = "qwen3.5:35b-a3b", Prompt = "test", Think = true };
			var json = JsonSerializer.Serialize(request);
			json.ShouldContain("\"think\":true");
		}

		[Test]
		public void Serializes_Think_Property_As_String()
		{
			var request = new GenerateRequest { Model = "qwen3.5:35b-a3b", Prompt = "test", Think = "always" };
			var json = JsonSerializer.Serialize(request);
			json.ShouldContain("\"think\":\"always\"");
		}

		[Test]
		public void Omits_Think_Property_When_Null()
		{
			var request = new GenerateRequest { Model = "qwen3.5:35b-a3b", Prompt = "test" };
			var json = JsonSerializer.Serialize(request);
			json.ShouldNotContain("\"think\"");
		}

		[Test]
		public void Serializes_Width_Height_Steps()
		{
			var request = new GenerateRequest { Model = "sd", Prompt = "cat", Width = 512, Height = 768, Steps = 20 };
			var json = JsonSerializer.Serialize(request);
			json.ShouldContain("\"width\":512");
			json.ShouldContain("\"height\":768");
			json.ShouldContain("\"steps\":20");
		}

		[Test]
		public void Omits_Experimental_Properties_When_Null()
		{
			var request = new GenerateRequest { Model = "qwen3.5:35b-a3b", Prompt = "test" };
			var json = JsonSerializer.Serialize(request);
			json.ShouldNotContain("\"width\"");
			json.ShouldNotContain("\"height\"");
			json.ShouldNotContain("\"steps\"");
		}

		[Test]
		public void Context_Property_Still_Serializes()
		{
			var request = new GenerateRequest { Model = "qwen3.5:35b-a3b", Prompt = "test", Context = [1, 2, 3] };
			var json = JsonSerializer.Serialize(request);
			json.ShouldContain("\"context\":[1,2,3]");
		}
	}

	/// <summary>
	/// Tests for GenerateResponseStream deserialization.
	/// </summary>
	public class GenerateResponseStreamTests : ModelSerializationTests
	{
		[Test]
		public void Deserializes_Thinking_Property()
		{
			var json = """{"model":"qwen3.5:35b-a3b","response":"hi","thinking":"Let me think...","done":false}""";
			var response = JsonSerializer.Deserialize<GenerateResponseStream>(json);
			response.ShouldNotBeNull();
			response.Thinking.ShouldBe("Let me think...");
			response.Response.ShouldBe("hi");
		}

		[Test]
		public void Thinking_Is_Null_When_Not_Present()
		{
			var json = """{"model":"qwen3.5:35b-a3b","response":"hi","done":false}""";
			var response = JsonSerializer.Deserialize<GenerateResponseStream>(json);
			response.ShouldNotBeNull();
			response.Thinking.ShouldBeNull();
		}

		[Test]
		public void Deserializes_Image_Property()
		{
			var json = """{"model":"sd","response":"","image":"iVBORw0KGgo=","completed":5,"total":20,"done":false}""";
			var response = JsonSerializer.Deserialize<GenerateResponseStream>(json);
			response.ShouldNotBeNull();
			response.Image.ShouldBe("iVBORw0KGgo=");
			response.CompletedSteps.ShouldBe(5);
			response.TotalSteps.ShouldBe(20);
		}

		[Test]
		public void Image_Properties_Are_Null_When_Not_Present()
		{
			var json = """{"model":"qwen3.5:35b-a3b","response":"hi","done":false}""";
			var response = JsonSerializer.Deserialize<GenerateResponseStream>(json);
			response.ShouldNotBeNull();
			response.Image.ShouldBeNull();
			response.CompletedSteps.ShouldBeNull();
			response.TotalSteps.ShouldBeNull();
		}
	}

	/// <summary>
	/// Tests for GenerateDoneResponseStream deserialization.
	/// </summary>
	public class GenerateDoneResponseStreamTests : ModelSerializationTests
	{
		[Test]
		public void Deserializes_DoneReason_Property()
		{
			var json = """{"model":"qwen3.5:35b-a3b","response":"","done":true,"done_reason":"stop","context":[1,2],"total_duration":100,"load_duration":50,"prompt_eval_count":10,"prompt_eval_duration":20,"eval_count":5,"eval_duration":30}""";
			var response = JsonSerializer.Deserialize<GenerateDoneResponseStream>(json);
			response.ShouldNotBeNull();
			response.DoneReason.ShouldBe("stop");
		}

		[Test]
		public void DoneReason_Is_Null_When_Not_Present()
		{
			var json = """{"model":"qwen3.5:35b-a3b","response":"","done":true,"context":[1,2],"total_duration":100,"load_duration":50,"prompt_eval_count":10,"prompt_eval_duration":20,"eval_count":5,"eval_duration":30}""";
			var response = JsonSerializer.Deserialize<GenerateDoneResponseStream>(json);
			response.ShouldNotBeNull();
			response.DoneReason.ShouldBeNull();
		}
	}

	/// <summary>
	/// Tests for ShowModelRequest serialization.
	/// </summary>
	public class ShowModelRequestTests : ModelSerializationTests
	{
		[Test]
		public void Serializes_Verbose_Property()
		{
			var request = new ShowModelRequest { Model = "qwen3.5:35b-a3b", Verbose = true };
			var json = JsonSerializer.Serialize(request);
			json.ShouldContain("\"verbose\":true");
		}

		[Test]
		public void Omits_Verbose_When_Null()
		{
			var request = new ShowModelRequest { Model = "qwen3.5:35b-a3b" };
			var json = JsonSerializer.Serialize(request);
			json.ShouldNotContain("\"verbose\"");
		}
	}

	/// <summary>
	/// Tests for ShowModelResponse deserialization.
	/// </summary>
	public class ShowModelResponseTests : ModelSerializationTests
	{
		[Test]
		public void Deserializes_ModifiedAt_Property()
		{
			var json = """{"license":"MIT","modelfile":"# test","parameters":"","template":"","details":{"parent_model":"","format":"gguf","family":"llama","families":null,"parameter_size":"7B","quantization_level":"Q4_0"},"model_info":{"general.architecture":"llama"},"modified_at":"2024-05-14T23:33:07.4166573+08:00"}""";
			var response = JsonSerializer.Deserialize<ShowModelResponse>(json);
			response.ShouldNotBeNull();
			response.ModifiedAt.ShouldNotBeNull();
			response.ModifiedAt.Value.Year.ShouldBe(2024);
		}

		[Test]
		public void ModifiedAt_Is_Null_When_Not_Present()
		{
			var json = """{"license":"MIT","modelfile":"# test","parameters":"","template":"","details":{"parent_model":"","format":"gguf","family":"llama","families":null,"parameter_size":"7B","quantization_level":"Q4_0"},"model_info":{"general.architecture":"llama"}}""";
			var response = JsonSerializer.Deserialize<ShowModelResponse>(json);
			response.ShouldNotBeNull();
			response.ModifiedAt.ShouldBeNull();
		}
	}

	/// <summary>
	/// Tests for Model deserialization.
	/// </summary>
	public class ListModelsModelTests : ModelSerializationTests
	{
		[Test]
		public void Deserializes_ModelName_Property()
		{
			var json = """{"name":"qwen3.5:35b-a3b:latest","model":"qwen3.5:35b-a3b:latest","modified_at":"2024-05-14T15:33:07Z","size":3791811617,"digest":"abc123"}""";
			var model = JsonSerializer.Deserialize<Model>(json);
			model.ShouldNotBeNull();
			model.Name.ShouldBe("qwen3.5:35b-a3b:latest");
			model.ModelName.ShouldBe("qwen3.5:35b-a3b:latest");
		}

		[Test]
		public void ModelName_Is_Null_When_Not_Present()
		{
			var json = """{"name":"qwen3.5:35b-a3b:latest","modified_at":"2024-05-14T15:33:07Z","size":3791811617,"digest":"abc123"}""";
			var model = JsonSerializer.Deserialize<Model>(json);
			model.ShouldNotBeNull();
			model.Name.ShouldBe("qwen3.5:35b-a3b:latest");
			model.ModelName.ShouldBeNull();
		}
	}

	/// <summary>
	/// Tests for CreateModelRequest serialization (bug fix).
	/// </summary>
	public class CreateModelRequestTests : ModelSerializationTests
	{
		[Test]
		public void Serializes_Messages_As_Plural()
		{
			var request = new CreateModelRequest
			{
				Model = "test",
				Messages = [new Message(ChatRole.User, "hello")]
			};
			var json = JsonSerializer.Serialize(request);
			json.ShouldContain("\"messages\":");
			json.ShouldNotContain("\"message\":");
		}
	}

	/// <summary>
	/// Tests for SystemOneRequest serialization.
	/// </summary>
	public class SystemOneRequestTests : ModelSerializationTests
	{
		private static SystemOneRequest CreateRequest()
		{
			return new SystemOneRequest
			{
				Model = "nimble",
				State = "Our checkout has returned 500 errors since 9am.",
				Questions = new()
				{
					["label"] = new SystemOneChoiceQuestion
					{
						Instructions = "Which label fits this ticket?",
						Criteria = new()
						{
							["billing"] = "Payments and refunds",
							["bug"] = "Software errors",
							["account"] = "Login and account access"
						}
					},
					["urgent"] = new SystemOneNoulQuestion
					{
						Instructions = "Is this urgent?"
					},
					["severity"] = new SystemOneScoreQuestion
					{
						Instructions = "How severe is this?",
						Criteria = ["low", "medium", "high"]
					}
				}
			};
		}

		[Test]
		public void Serializes_Question_Type_Discriminators()
		{
			var json = JsonSerializer.Serialize(CreateRequest());

			json.ShouldContain("\"type\":\"choice\"");
			json.ShouldContain("\"type\":\"noul\"");
			json.ShouldContain("\"type\":\"score\"");
		}

		[Test]
		public void Serializes_State_And_Model()
		{
			var json = JsonSerializer.Serialize(CreateRequest());

			json.ShouldContain("\"model\":\"nimble\"");
			json.ShouldContain("\"state\":\"Our checkout has returned 500 errors since 9am.\"");
			json.ShouldContain("\"questions\":");
		}

		[Test]
		public void Does_Not_Serialize_The_Type_Property_Twice()
		{
			var json = JsonSerializer.Serialize(CreateRequest());

			json.Contains("\"Type\"").ShouldBeFalse();
		}

		[Test]
		public void Serializes_Noul_Criteria_Using_False_And_True_Keys()
		{
			var request = new SystemOneRequest
			{
				Model = "nimble",
				State = "state",
				Questions = new()
				{
					["q"] = new SystemOneNoulQuestion
					{
						Instructions = "Is this urgent?",
						Criteria = new SystemOneNoulCriteria { No = "Not urgent", Yes = "Urgent" }
					}
				}
			};

			var json = JsonSerializer.Serialize(request);

			json.ShouldContain("\"false\":\"Not urgent\"");
			json.ShouldContain("\"true\":\"Urgent\"");
		}

		[Test]
		public void Omits_KeepAlive_When_Null()
		{
			var json = JsonSerializer.Serialize(CreateRequest());

			json.ShouldNotContain("\"keep_alive\"");
		}

		[Test]
		public void Deserializes_Questions_Into_Derived_Types()
		{
			var json = JsonSerializer.Serialize(CreateRequest());

			var request = JsonSerializer.Deserialize<SystemOneRequest>(json);

			request.ShouldNotBeNull();
			request.Questions["label"].ShouldBeOfType<SystemOneChoiceQuestion>();
			request.Questions["urgent"].ShouldBeOfType<SystemOneNoulQuestion>();
			request.Questions["severity"].ShouldBeOfType<SystemOneScoreQuestion>();
		}

		[Test]
		public void RoundTrips_With_Source_Generated_Context()
		{
			var request = CreateRequest();

			var json = JsonSerializer.Serialize(request, JsonSourceGenerationContext.Default.SystemOneRequest);
			var deserialized = JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.SystemOneRequest);

			deserialized.ShouldNotBeNull();
			deserialized.Model.ShouldBe("nimble");
			deserialized.State.ShouldBeOfType<JsonElement>().GetString().ShouldBe("Our checkout has returned 500 errors since 9am.");
			deserialized.Questions["label"].ShouldBeOfType<SystemOneChoiceQuestion>();
			deserialized.Questions["urgent"].ShouldBeOfType<SystemOneNoulQuestion>();
			deserialized.Questions["severity"].ShouldBeOfType<SystemOneScoreQuestion>();
		}
	}

	/// <summary>
	/// Tests for SystemOneResponse deserialization.
	/// </summary>
	public class SystemOneResponseTests : ModelSerializationTests
	{
		private const string ResponseJson = """
		{
			"model": "nimble",
			"answers": {
				"label": {
					"type": "choice",
					"choice": "bug",
					"probabilities": { "billing": 0.0125, "bug": 0.9781, "account": 0.0093 },
					"confidence": 0.8906
				},
				"urgent": {
					"type": "noul",
					"noul": 0.87
				},
				"severity": {
					"type": "score",
					"score": 1.5,
					"legend": { "0": "low", "1": "medium", "2": "high" },
					"probabilities": { "0": 0.1, "1": 0.3, "2": 0.6 },
					"confidence": 0.44
				}
			},
			"usage": { "input_tokens": 174, "output_tokens": 1 }
		}
		""";

		[Test]
		public void Deserializes_Model_And_Usage()
		{
			var response = JsonSerializer.Deserialize<SystemOneResponse>(ResponseJson);

			response.ShouldNotBeNull();
			response.Model.ShouldBe("nimble");
			response.Usage.InputTokens.ShouldBe(174);
			response.Usage.OutputTokens.ShouldBe(1);
		}

		[Test]
		public void Deserializes_Choice_Answer()
		{
			var response = JsonSerializer.Deserialize<SystemOneResponse>(ResponseJson);

			var answer = response.Answers["label"].ShouldBeOfType<SystemOneChoiceAnswer>();
			answer.Type.ShouldBe("choice");
			answer.Choice.ShouldBe("bug");
			answer.Probabilities["bug"].ShouldBe(0.9781);
			answer.Confidence.ShouldBe(0.8906);
		}

		[Test]
		public void Deserializes_Noul_Answer()
		{
			var response = JsonSerializer.Deserialize<SystemOneResponse>(ResponseJson);

			var answer = response.Answers["urgent"].ShouldBeOfType<SystemOneNoulAnswer>();
			answer.Noul.ShouldBe(0.87);
		}

		[Test]
		public void Deserializes_Score_Answer()
		{
			var response = JsonSerializer.Deserialize<SystemOneResponse>(ResponseJson);

			var answer = response.Answers["severity"].ShouldBeOfType<SystemOneScoreAnswer>();
			answer.Score.ShouldBe(1.5);
			answer.Legend["2"].ShouldBe("high");
			answer.Probabilities["2"].ShouldBe(0.6);
			answer.Confidence.ShouldBe(0.44);
		}
	}
}

#pragma warning restore CS0618
#pragma warning restore CS8602
#pragma warning restore CS8604
