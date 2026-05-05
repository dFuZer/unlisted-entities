using Zorro.Core;
using Zorro.Core.Serizalization;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class SilverFulminateExplosionContentEvent : ContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_SilverFulminateExplosion_0",
		"Content_SilverFulminateExplosion_1",
		"Content_SilverFulminateExplosion_2",
		"Content_SilverFulminateExplosion_3",
	};

	public override float GetContentValue() => 75f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "SilverFulminateExplosion";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());

	public override void Serialize(BinarySerializer serializer) { }

	public override void Deserialize(BinaryDeserializer deserializer) { }
}
