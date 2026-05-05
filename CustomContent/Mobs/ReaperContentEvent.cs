using Zorro.Core;

namespace UnlistedEntities.CustomContent;

public class ReaperContentEvent : MonsterContentEvent
{
	public static string[] NORMAL_COMMENTS = new string[14]
	{
		"Content_Reaper_0",
		"Content_Reaper_1",
		"Content_Reaper_2",
		"Content_Reaper_3",
		"Content_Reaper_4",
		"Content_Reaper_5",
		"Content_Reaper_6",
		"Content_Reaper_7",
		"Content_Reaper_8",
		"Content_Reaper_9",
		"Content_Reaper_10",
		"Content_Reaper_11",
		"Content_Reaper_12",
		"Content_Reaper_13",
	};

	public override float GetContentValue()
	{
		return 75;
	}

	public override ushort GetID()
	{
		return DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);
	}

	public override string GetName()
	{
		return "Reaper";
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
