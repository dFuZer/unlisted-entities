using Zorro.Core;

namespace UnlistedEntities.CustomContent;


public class TeapotContentEvent : MonsterContentEvent
{
	public static string[] NORMAL_COMMENTS = new string[11]
	{
		"Is that a teapot?",
        "A teapot??",
        "wtf a teapot with legs lol",
        "careful lmaoo its hot",
        "I hate its little legs",
        "use him to make tea lol",
        "Love those videos, please make more, cheers !",
        "is the teapot dangerous?",
        "Teapot teapot teapot!",
        "Hahaha its boiling hot!",
		"teapot looks dangerous, careful"
	};

	public override float GetContentValue()
	{
		return 55;
	}

	public override ushort GetID()
	{
		return DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);
	}

	public override string GetName()
	{
		return "Teapot";
	}

	public override Comment GenerateComment()
	{
		return new Comment(NORMAL_COMMENTS.GetRandom());
	}
}
