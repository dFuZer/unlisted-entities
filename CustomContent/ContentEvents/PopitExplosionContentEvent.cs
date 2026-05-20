using Zorro.Core;
using Zorro.Core.Serizalization;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class PopitExplosionContentEvent : ContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_PopitExplosion_0",
		"Content_PopitExplosion_1",
		"Content_PopitExplosion_2",
		"Content_PopitExplosion_3",
	};

	public override float GetContentValue() => 5f;

	public override ushort GetID() => DbsContentApi.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "PopitExplosion";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());

	public override void Serialize(BinarySerializer serializer) { }

	public override void Deserialize(BinaryDeserializer deserializer) { }
}
