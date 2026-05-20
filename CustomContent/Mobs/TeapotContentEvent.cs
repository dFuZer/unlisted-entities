using Zorro.Core;

namespace UnlistedEntities.CustomContent;


public class TeapotContentEvent : MonsterContentEvent
{
	public static string[] NORMAL_COMMENTS = new string[15]
	{
		"Content_Teapot_0",
		"Content_Teapot_1",
		"Content_Teapot_2",
		"Content_Teapot_3",
		"Content_Teapot_4",
		"Content_Teapot_5",
		"Content_Teapot_6",
		"Content_Teapot_7",
		"Content_Teapot_8",
		"Content_Teapot_9",
		"Content_Teapot_10",
		"Content_Teapot_11",
		"Content_Teapot_12",
		"Content_Teapot_13",
		"Content_Teapot_14",
	};

	public override float GetContentValue()
	{
		return 55;
	}

	public override ushort GetID()
	{
		return DbsContentApi.ContentEvents.GetEventID(GetType().Name);
	}

	public override string GetName()
	{
		return "Teapot";
	}

	public override Comment GenerateComment()
	{
		return new Comment(NORMAL_COMMENTS.GetRandom());
	}

	public override string[] GetAllComments()
	{
		return NORMAL_COMMENTS;
	}
}
