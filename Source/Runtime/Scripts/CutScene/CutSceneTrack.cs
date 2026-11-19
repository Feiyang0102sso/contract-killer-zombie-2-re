using System.Collections.Generic;

public sealed class CutSceneTrack : MonoBehaviourAndLog
{
	private readonly List<CutSceneCommand> m_Commands = new List<CutSceneCommand>();

	public void AddCommand(CutSceneCommand command)
	{
		m_Commands.Add(command);
	}

	public bool IsValid
	{
		get
		{
			return m_Commands.Count > 0;
		}
	}

	public void Begin()
	{
		m_Commands[0].Begin();
	}

	public bool Next()
	{
		if (!m_Commands[0].Next())
		{
			m_Commands[0].End();
			m_Commands.RemoveAt(0);
			if (IsValid)
			{
				m_Commands[0].Begin();
			}
		}
		return IsValid;
	}
}
