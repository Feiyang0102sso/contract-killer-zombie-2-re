using System.Collections.Generic;
using Glu.AssetBundles;
using Glu.Localization;
using UnityEngine;

public class DailyBonusPopup : UIWindow
{
	private const float topOffset = 18.5f;

	public UIScrollList List;

	public UIExpandSprite Back;

	public UIButton OkButton;

	public SpriteText TitleText;

	public SpriteText Description;

	public GameObject LastDayPlacer;

	public UISingleSprite Background;

	public PackedSprite ArrowLeft;

	public PackedSprite ArrowRight;

	public SpriteText TodayBonusText;

	private List<DailyBonusItemPlate> _items = new List<DailyBonusItemPlate>();

	protected override void Awake()
	{
		_position = new Vector2(0f, -9.25f);
		base.Awake();
		List.viewableArea.x = GUIUtils.ScreenWidth() * 0.75f;
		TexturesReplacer.ReplaceTexturesAccordingToQuality();
	}

	public void Init(int day)
	{
		int num = Singleton.GameData.DailyBonus.Length;
		day %= num;
		TodayBonusText.Text = string.Concat(GUIUtils.ColorYellowTitle, Strings.GetString("TODAY YOUR BONUS IS: "), Color.white, GetBonusText(Singleton.GameData.DailyBonus[day]));
		GameObject original = Resources.Load("DailyBonusItemPlate") as GameObject;
		for (int i = 0; i < Singleton.GameData.DailyBonus.Length - 1; i++)
		{
			DailyBonusItemPlate component = (Object.Instantiate(original) as GameObject).GetComponent<DailyBonusItemPlate>();
			component.Init(i, day);
			_items.Add(component);
			AddListItem(component.transform);
		}
		List.ScrollToItem(day, 0f);
		List.LateUpdate();
		List.ScrollListTo(List.ScrollPosition - 0.3f / (float)num);
		if (day == num - 1)
		{
			LastDayPlacer.transform.GetComponent<UIAlign>().m_margin.x = 0f;
			LastDayPlacer.transform.GetComponent<UIAlign>().m_horizontal = UIAlign.Horizontal.Center;
			Object.Destroy(List.gameObject);
			ArrowLeft.gameObject.active = false;
			ArrowRight.gameObject.active = false;
		}
		DailyBonusItemPlate component2 = (Object.Instantiate(original) as GameObject).GetComponent<DailyBonusItemPlate>();
		component2.AlphaOnIcon = false;
		component2.SpecialYellowGlowing = true;
		component2.Init(num - 1, day);
		component2.transform.parent = LastDayPlacer.transform;
		component2.transform.localPosition = Vector3.zero;
		GUIUtils.SetupUIHighlightShader(component2.Icon.renderer.material, 0.23f, 2f, 1.5f, Color.white);
		Description.Text = Util.SafeFormat(Strings.GetString("Complete one contract mission each day to earn a bonus!\n Play {0} days in-a-row to win a {1}"), num, GetBonusText(Singleton.GameData.DailyBonus[num - 1]));
		GiveDailyBonus(day, Singleton.GameData.DailyBonus[day]);
		TitleText.gameObject.AddComponent<UIAnimationScale>().Setup(1.6f, 1f, 0.2f, 0.7f, EZAnimation.EASING_TYPE.BackIn);
		TitleText.gameObject.AddComponent<UIAnimationAlpha>().Setup(0.5f, 1f, 0.2f, 0.7f, EZAnimation.EASING_TYPE.BackIn);
		TitleText.gameObject.AddComponent<UIAnimationMove>().Setup(0f, -10f, 0.2f, 0.7f, EZAnimation.EASING_TYPE.BackIn);
	}

	protected override void Start()
	{
		base.Start();
		Back.SetSize(GUIUtils.ScreenWidth(), GUIUtils.ScreenHeight() - 18.5f);
		Background.Clipped = true;
		Rect r = new Rect();
		r.width = GUIUtils.ScreenWidth() - 5f;
		r.height = GUIUtils.ScreenHeight();
		r.center = Vector3.zero;
		Background.ClippingRect = new Rect3D(r);
		GUIUtils.HookButton(OkButton, Close);
		UpdateItemsFadeEffect(true);
	}

	private void AddListItem(Transform item)
	{
		GameObject gameObject = new GameObject("element");
		UIListItemContainer uIListItemContainer = gameObject.AddComponent<UIListItemContainer>();
		item.parent = uIListItemContainer.transform;
		item.localPosition = Vector3.zero;
		uIListItemContainer.FindOuterEdges();
		Vector2 bottomRightEdge = uIListItemContainer.BottomRightEdge;
		List.InsertItem(uIListItemContainer.gameObject.GetComponent(typeof(IUIListObject)) as IUIListObject, List.Count);
	}

	private void Update()
	{
		UpdateItemsFadeEffect(false);
	}

	private void UpdateItemsFadeEffect(bool all)
	{
		if (!List)
		{
			return;
		}
		float x = List.transform.position.x;
		foreach (DailyBonusItemPlate item in _items)
		{
			if (!(item.gameObject.active || all))
			{
				continue;
			}
			Transform transform = item.transform;
			float num = Mathf.Lerp(1f, 0.5f, Mathf.Pow(Mathf.Abs((transform.position.x - x) / (List.viewableArea.x * 0.5f)), 5f));
			transform.localScale = new Vector3(num, num, num);
			float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(Mathf.Abs((transform.position.x - x) / (List.viewableArea.x * 0.5f)), 5f));
			item.SetAlpha(alpha);
		}
	}

	public static void SetupBonusTexture(UISingleSprite icon, GameData.DailyBonusData data, bool allowRotation)
	{
		Texture2D texture = null;
		icon.width = 45f;
		icon.height = 45f;
		icon.transform.localRotation = Quaternion.identity;
		if (data.Item.Length > 0)
		{
			CommonShopItem commonShopItemByItem = Singleton.ShopData.GetCommonShopItemByItem(data.Item);
			if (Singleton.PlayerData.HasWeapon(commonShopItemByItem.item) && data.AlternativeItem.Length > 0)
			{
				commonShopItemByItem = Singleton.ShopData.GetCommonShopItemByItem(data.AlternativeItem);
			}
			if (commonShopItemByItem is WeaponShopItem)
			{
				if (allowRotation)
				{
					icon.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -60f));
					icon.transform.localPosition = new Vector3(-6f, 15f, -2f);
				}
				icon.width = 80f;
				icon.height = 40f;
			}
			else if (commonShopItemByItem is ConsumableShopItem)
			{
			}
			if (commonShopItemByItem != null)
			{
				texture = AssetBundles.Load(commonShopItemByItem.image) as Texture2D;
			}
		}
		if (data.SoftCurrency > 0)
		{
			texture = AssetBundles.Load("Assets/UI/Shop/IAPs/bucks_01.png") as Texture2D;
		}
		else if (data.HardCurrency > 0)
		{
			texture = AssetBundles.Load("Assets/UI/Shop/IAPs/gold_01.png") as Texture2D;
		}
		icon.SetTexture(texture);
	}

	public static string GetBonusText(GameData.DailyBonusData data)
	{
		bool isMoney;
		return GetBonusText(data, out isMoney);
	}

	public static string GetBonusText(GameData.DailyBonusData data, out bool isMoney)
	{
		isMoney = false;
		if (data.Item.Length > 0)
		{
			CommonShopItem commonShopItemByItem = Singleton.ShopData.GetCommonShopItemByItem(data.Item);
			if (Singleton.PlayerData.HasWeapon(commonShopItemByItem.item) && data.AlternativeItem.Length > 0)
			{
				commonShopItemByItem = Singleton.ShopData.GetCommonShopItemByItem(data.AlternativeItem);
			}
			if (commonShopItemByItem != null)
			{
				return Strings.GetString(commonShopItemByItem.nameId);
			}
		}
		if (data.SoftCurrency > 0)
		{
			isMoney = true;
			return GUIUtils.GetPriceStringForCurrency(data.SoftCurrency, "soft");
		}
		if (data.HardCurrency > 0)
		{
			isMoney = true;
			return GUIUtils.GetPriceStringForCurrency(data.HardCurrency, "hard");
		}
		return null;
	}

	public static void GiveDailyBonus(int daysPlayed, GameData.DailyBonusData data)
	{
		if (data.Item.Length > 0)
		{
			CommonShopItem commonShopItemByItem = Singleton.ShopData.GetCommonShopItemByItem(data.Item);
			if (Singleton.PlayerData.HasWeapon(commonShopItemByItem.item) && data.AlternativeItem.Length > 0)
			{
				commonShopItemByItem = Singleton.ShopData.GetCommonShopItemByItem(data.AlternativeItem);
			}
			if (commonShopItemByItem != null)
			{
				if (commonShopItemByItem is WeaponShopItem)
				{
					Singleton.PlayerData.AddWeapon((commonShopItemByItem as WeaponShopItem).item);
					GameAnalytics.LogDailyBonus(daysPlayed, (commonShopItemByItem as WeaponShopItem).item);
				}
				else if (commonShopItemByItem is ConsumableShopItem)
				{
					Singleton.PlayerData.AddConsumable((commonShopItemByItem as ConsumableShopItem).item, 1);
					GameAnalytics.LogDailyBonus(daysPlayed, (commonShopItemByItem as ConsumableShopItem).item);
				}
			}
		}
		if (data.SoftCurrency > 0)
		{
			Singleton.PlayerData.SoftCurrency += data.SoftCurrency;
			GameAnalytics.LogDailyBonus(daysPlayed, data.SoftCurrency.ToString() + " Soft");
		}
		else if (data.HardCurrency > 0)
		{
			Singleton.PlayerData.HardCurrency += data.HardCurrency;
			GameAnalytics.LogDailyBonus(daysPlayed, data.HardCurrency.ToString() + " Hard");
			GameAnalytics.LogHCReceived(data.HardCurrency, GameAnalytics.HCSourceType.Game);
		}
	}
}
