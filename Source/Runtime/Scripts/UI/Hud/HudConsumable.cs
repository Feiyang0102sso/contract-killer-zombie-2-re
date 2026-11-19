using System;
using UnityEngine;

public class HudConsumable : MonoBehaviour
{
	public SimpleSprite _sprite;

	public UICircularBar _circularBar;

	private Color FadeColor = new Color(1f, 1f, 1f, 0.5f);

	private Consumable _consumable;

	public Consumable Consumable
	{
		set
		{
			if ((bool)_consumable)
			{
				_consumable.OnConsume -= new Action(OnConsume);
			}
			_consumable = value;
			_consumable.OnConsume += new Action(OnConsume);
		}
		get
		{
			return _consumable;
		}
	}

	private void Start()
	{
		string str = gameObject.name;
		Util.TryRemovePostfix(ref str, "(Clone)");
	}

	private void OnConsume()
	{
		enabled = true;
		_circularBar.GetComponent<Renderer>().enabled = true;
	}

	private void Update()
	{
		float rechargeTimeLeft = _consumable.GetRechargeTimeLeft();
		_sprite.SetColor(rechargeTimeLeft > 0f ? FadeColor : Color.white);
		if (rechargeTimeLeft <= 0f)
		{
			enabled = false;
			_circularBar.GetComponent<Renderer>().enabled = false;
		}
		_circularBar.Value = rechargeTimeLeft / _consumable.Cooldown;
	}

	private void OnDestroy()
	{
		if ((bool)_consumable)
		{
			_consumable.OnConsume -= new Action(OnConsume);
		}
	}
}
