using System;
using Glu.Localization;
using UnityEngine;

public class StoreGroupButton : MonoBehaviour
{
	public float height;

	public UIButton Button;

	public SpriteText NameLabel;

	private StoreGroupContainer _StoreGroupContainer;

	private ShopConfigGroup _shopConfigGroup;

	public ShopConfigGroup ShopConfigGroup
	{
		get
		{
			return _shopConfigGroup;
		}
	}

	private void Awake()
	{
	}

	public void Init(ShopConfigGroup shopConfigGroup, StoreGroupContainer container, float animationDeltaY, bool allowSaleBadge)
	{
		_shopConfigGroup = shopConfigGroup;
		_StoreGroupContainer = container;
		NameLabel.Text = Strings.GetString(shopConfigGroup.nameId);
		foreach (ShopConfigGroup.Reference itemRef in shopConfigGroup.itemRefs)
		{
			CommonShopItem commonShopItem = itemRef.item as CommonShopItem;
			if (commonShopItem != null && commonShopItem.readyToShow && commonShopItem.IsOnSale() && allowSaleBadge)
			{
				GameObject gameObject = Util.CreateGameObject("ShopGroupSaleBadge");
				gameObject.transform.parent = Button.transform;
				gameObject.transform.localPosition = new Vector3(Button.width * 0.9f, (-Button.height) * 0.5f, -2f);
			}
		}
		if (animationDeltaY < 0f)
		{
			base.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 0f, 0.4f, EZAnimation.EASING_TYPE.BackIn).RunAnimation();
			base.gameObject.AddComponent<UIAnimationScale>().Setup(1.1f, 1f, 0f, 0.6f, EZAnimation.EASING_TYPE.BackIn).RunAnimation();
			base.gameObject.AddComponent<UIAnimationMove>().Setup(0f, animationDeltaY, 0f, 0.6f, EZAnimation.EASING_TYPE.BackIn).RunAnimation();
		}
	}

	public void SetOnClickDelegate(Action<StoreGroupButton> onClickDelegate)
	{
		GUIUtils.HookButton(Button, () =>
		{
			if (gameObject.GetComponent<UIAnimation>())
			{
				return;
			}
			onClickDelegate(this);
		});
	}

	public void BringIn()
	{
		int count = _StoreGroupContainer.groupButtons.Count;
		int num = _StoreGroupContainer.groupButtons.IndexOf(this);
		float unusedAnimationTime;
		float num2 = 0.3f / (float)count;
		float delay = num2 * (float)num;
		gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, delay, 0.3f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
		gameObject.AddComponent<UIAnimationMove>().Setup((-Button.width) * 0.5f, 0f, delay, 0.3f, EZAnimation.EASING_TYPE.Spring).RunAnimation();
	}

	public void FadeOut()
	{
		int count = _StoreGroupContainer.groupButtons.Count;
		int num = _StoreGroupContainer.groupButtons.IndexOf(this);
		float unusedAnimationTime;
		float num2 = 0.5f / (float)count;
		float num3 = num2 * (float)(count - 1 - num);
		gameObject.AddComponent<UIAnimationAlpha>().Setup(1f, 0f, num3, 0.5f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
		gameObject.AddComponent<UIAnimationScale>().Setup(1f, 2f, num3, 0.35f, EZAnimation.EASING_TYPE.ExponentialIn).RunAnimation();
		transform.localPosition -= new Vector3(Button.width * 0.5f, (-Button.height) * 0.5f, 0f);
		gameObject.AddComponent<UIAnimationMove>().Setup(Button.width * 0.5f, (-Button.height) * 0.5f, num3, 0.35f, EZAnimation.EASING_TYPE.ExponentialIn).RunAnimation();
		UnityEngine.Object.Destroy(gameObject, num3 + 0.5f);
	}
}
