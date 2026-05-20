using Zorro.Core;
using Zorro.Core.Serizalization;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class SemtexExplosionContentEvent : ContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_SemtexExplosion_0",
		"Content_SemtexExplosion_1",
		"Content_SemtexExplosion_2",
		"Content_SemtexExplosion_3",
	};

	public override float GetContentValue() => 20f;

	public override ushort GetID() => DbsContentApi.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "SemtexExplosion";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());

	public override void Serialize(BinarySerializer serializer) { }

	public override void Deserialize(BinaryDeserializer deserializer) { }
}
