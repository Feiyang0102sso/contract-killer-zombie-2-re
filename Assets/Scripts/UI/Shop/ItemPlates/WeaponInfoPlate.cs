using Glu.Localization;
using UnityEngine;

public class WeaponInfoPlate : WeaponStoreFrontItemPlate
{
	private class Param
	{
		public TripleProgressBar bar;

		public SpriteText label;

		public string name;

		public float myValue;

		public float compareValue;
	}

	public GameObject ParamsAnchor;

	public SpriteText ParamNameText;

	public float ParamsSpacingY = -10f;

	public float ParamsSpacingX = 2f;

	private Param[] _weaponParams;

	protected override bool AllowSaleBadge()
	{
		return false;
	}

	public void Init(WeaponShopItem thisWeapon, WeaponShopItem anotherWeapon)
	{
		base.Init(thisWeapon);
		if (anotherWeapon == null)
		{
			anotherWeapon = thisWeapon;
		}
		_weaponParams = new Param[6]
		{
			new Param
			{
				name = Strings.GetString("DAMAGE:"),
				myValue = thisWeapon.Dmg,
				compareValue = anotherWeapon.Dmg
			},
			new Param
			{
				name = Strings.GetString("ACCURACY:"),
				myValue = thisWeapon.Acc,
				compareValue = anotherWeapon.Acc
			},
			new Param
			{
				name = Strings.GetString("RANGE:"),
				myValue = thisWeapon.Range,
				compareValue = anotherWeapon.Range
			},
			new Param
			{
				name = Strings.GetString("RATE OF FIRE:"),
				myValue = thisWeapon.Rate,
				compareValue = anotherWeapon.Rate
			},
			new Param
			{
				name = Strings.GetString("MAX AMMO:"),
				myValue = thisWeapon.Cap,
				compareValue = anotherWeapon.Cap
			},
			new Param
			{
				name = Strings.GetString("ATTACK SPEED:"),
				myValue = thisWeapon.Speed,
				compareValue = anotherWeapon.Speed
			}
		};
		UpdateParams();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void UpdateState()
	{
		base.UpdateState();
		if (_weaponShopItem == null)
		{
			return;
		}
		bool flag = _weaponShopItem.IsOnSale() && _weaponShopItem.SalePrice == 0;
		if (flag)
		{
			Object.Destroy(BuyButton.spriteText.GetComponent<LocalizationComponent>());
			BuyButton.spriteText.Text = Strings.GetString("FREE");
			if (PriceText != null)
			{
				GUIUtils.SetupPriceText(PriceText, _weaponShopItem.BasePrice, _weaponShopItem.currency);
			}
		}
		SaleLine.gameObject.SetActiveRecursively(flag);
	}

	private void UpdateParams()
	{
		int num = 0;
		for (int i = 0; i < _weaponParams.Length; i++)
		{
			Param param = _weaponParams[i];
			if (param.myValue == 0f)
			{
				continue;
			}
			if (param.bar == null)
			{
				param.bar = Util.CreateGameObject("TripleProgressBar").GetComponent<TripleProgressBar>();
				param.bar.transform.parent = ParamsAnchor.transform;
				param.bar.transform.localPosition = new Vector3(ParamsSpacingX * 0.5f + param.bar.Width * 0.5f, (float)num * ParamsSpacingY, 0f);
				if (num > 0)
				{
					param.label = (Object.Instantiate(ParamNameText.gameObject) as GameObject).GetComponent<SpriteText>();
				}
				else
				{
					param.label = ParamNameText;
				}
				param.label.transform.parent = ParamsAnchor.transform;
				param.label.Anchor = SpriteText.Anchor_Pos.Middle_Right;
				param.label.transform.localPosition = new Vector3((-ParamsSpacingX) * 0.5f, (float)num * ParamsSpacingY, 0f);
				param.label.Text = param.name;
			}
			param.bar.Value = param.compareValue * 0.01f;
			param.bar.AdditionalValue = (param.myValue - param.compareValue) * 0.01f;
			num++;
		}
	}
}
