using Microsoft.Extensions.AI;

namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// A builder that can append <see cref="ChatResponseUpdate"/> to one single completion update
/// </summary>
internal class ChatResponseUpdateBuilder
{
	private ChatResponseUpdate? _first;

	/// <summary>
	/// Appends a completion update to build one single completion update item
	/// </summary>
	/// <param name="update">The completion update to append to the final completion update</param>
	public void Append(ChatResponseUpdate? update)
	{
		if (update is null)
			return;

		_first ??= update;

		_first.AdditionalProperties = update.AdditionalProperties;
		_first.AuthorName = update.AuthorName;
		_first.CreatedAt = update.CreatedAt;
		_first.FinishReason = update.FinishReason;
		_first.ResponseId = update.ResponseId;
		_first.Role = update.Role;

		//_first.Contents will be set in Complete() with values collected from each update
		//_first.RawRepresentation makes no sense 

		// TODO: Check if this can ever be null. The docs imply not.
		if (update.Contents is not null)
			Contents.AddRange(update.Contents);
	}

	/// <summary>
	/// Builds the final consolidated <see cref="ChatResponseUpdate"/> out of the streamed
	/// updates that were appended before
	/// </summary>
	/// <returns>The final consolidated <see cref="ChatResponseUpdate"/> object</returns>
	public ChatResponseUpdate? Complete()
	{
		if (_first is null)
			return null;

		_first.Contents = Contents;

		return _first;
	}

	/// <summary>
	/// Gets or sets the list of all content elements received from completion updates
	/// </summary>
	/// <value>A <see cref="List{AIContent}"/> of <see cref="AIContent"/> elements</value>
	public List<AIContent> Contents { get; set; } = [];
}