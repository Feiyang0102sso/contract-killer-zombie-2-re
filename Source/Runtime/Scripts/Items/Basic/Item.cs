using UnityEngine;

public abstract class Item : MonoBehaviour
{
	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	public virtual bool IsUsedInGame()
	{
		return true;
	}

	public virtual void Stack(int count)
	{
	}

	public virtual GameObject CreateHudIcon()
	{
		return null;
	}
}
