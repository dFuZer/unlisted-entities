using Zorro.Core;
using Zorro.Core.Serizalization;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Parameterless content events: no player names, actor numbers, or Photon view IDs in the binary payload.
/// </summary>
public abstract class SimpleContentEvent : ContentEvent
{
	protected abstract string[] Comments { get; }
	protected abstract float Value { get; }
	protected abstract string DisplayName { get; }

	public sealed override float GetContentValue() => Value;

	public sealed override ushort GetID() =>
		DbsContentApi.ContentEvents.GetEventID(GetType().Name);

	public sealed override string GetName() => DisplayName;

	public sealed override string[] GetAllComments() => Comments;

	public sealed override Comment GenerateComment() =>
		new Comment(Comments.GetRandom());

	public sealed override void Serialize(BinarySerializer serializer) { }

	public sealed override void Deserialize(BinaryDeserializer deserializer) { }
}
