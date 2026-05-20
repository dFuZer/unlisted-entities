using Zorro.Core;
using Zorro.Core.Serizalization;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class GrenadeExplosionContentEvent : ContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_GrenadeExplosion_0",
		"Content_GrenadeExplosion_1",
		"Content_GrenadeExplosion_2",
		"Content_GrenadeExplosion_3",
	};

	public override float GetContentValue() => 55f;

	public override ushort GetID() => DbsContentApi.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "GrenadeExplosion";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());

	public override void Serialize(BinarySerializer serializer) { }

	public override void Deserialize(BinaryDeserializer deserializer) { }
}
