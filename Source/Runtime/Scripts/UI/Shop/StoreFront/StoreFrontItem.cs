using UnityEngine;

public abstract class StoreFrontItem : MonoBehaviour
{
	private bool _started;

	private bool _enabled;

	public abstract void Init(ShopConfigItem item);

	protected virtual void LateInit()
	{
	}

	protected virtual void Start()
	{
		_started = true;
		if (_started && _enabled)
		{
			LateInit();
		}
	}

	protected virtual void OnEnable()
	{
		_enabled = true;
		if (_started && _enabled)
		{
			LateInit();
		}
	}

	public virtual void OnBeforeRecalculatingBounds()
	{
	}

	public virtual void OnAfterRecalculatingBounds()
	{
	}
}
