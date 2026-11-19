using System;
using UnityEngine;

public static class GUIUtils
{
	public delegate void ButtonActionDelegate();

	public const float EZ_CAMERA_SIZE = 100f;

	public static Vector3 PositionOverAll = new Vector3(0f, 0f, -5f);

	public static Camera _uiCamera = null;

	public static bool InputEnabled = true;

	private static UIButton _lastPressedButton;

	public static Color ColorGreenUI = new Color(4f / 15f, 213f / 255f, 4f / 15f);

	public static Color ColorGreyUI = new Color(132f / 255f, 132f / 255f, 132f / 255f);

	public static Color ColorYellowHard = new Color(220f / 255f, 186f / 255f, 1f / 15f);

	public static Color ColorBlueTitle = new Color(162f / 255f, 177f / 255f, 1f);

	public static Color ColorYellowTitle = ColorYellowHard;

	public static string HardCurrencySymbol = "^";

	public static string SoftCurrencySymbol = "@";

	public static float Aspect()
	{
		return (float)Screen.width * 1f / (float)Screen.height;
	}

	public static float ScreenHeight()
	{
		return 200f;
	}

	public static float ScreenWidth()
	{
		return ScreenHeight() * Aspect();
	}

	public static float Pixels2GUIUnits(float pixels)
	{
		return pixels * ScreenHeight() / 1280f;
	}

	public static Camera UICamera()
	{
		if (_uiCamera == null)
		{
			EZCameraSettings[] uiCameras = UIManager.instance.uiCameras;
			if (uiCameras.Length == 0)
			{
				return null;
			}
			_uiCamera = uiCameras[0].camera;
		}
		return _uiCamera;
	}

	public static void OverHookButton(UIButton button, ButtonActionDelegate actionDelegate)
	{
		HookButton(button, actionDelegate, POINTER_INFO.INPUT_EVENT.RELEASE, false);
	}

	public static void HookButton(UIButton button, ButtonActionDelegate actionDelegate)
	{
		HookButton(button, actionDelegate, POINTER_INFO.INPUT_EVENT.RELEASE, true);
	}

	public static void HookButton(UIButton button, ButtonActionDelegate actionDelegate, bool playSound)
	{
		HookButton(button, actionDelegate, POINTER_INFO.INPUT_EVENT.RELEASE, true, playSound);
	}

	public static void HookButton(UIButton button, ButtonActionDelegate actionDelegate, POINTER_INFO.INPUT_EVENT inputEvent, bool addDelegate)
	{
		bool unusedLocal1;
		HookButton(button, actionDelegate, inputEvent, addDelegate, true);
	}

	public static void ClearCaches()
	{
		_lastPressedButton = null;
	}

	public static void HookButton(UIButton button, ButtonActionDelegate actionDelegate, POINTER_INFO.INPUT_EVENT inputEvent, bool addDelegate, bool playSound)
	{
		EZInputDelegate eZInputDelegate = delegate(ref POINTER_INFO ptr)
		{
			POINTER_INFO.INPUT_EVENT iNPUT_EVENT = ptr.evt;
			if (iNPUT_EVENT == POINTER_INFO.INPUT_EVENT.TAP)
			{
				iNPUT_EVENT = POINTER_INFO.INPUT_EVENT.RELEASE;
			}
			else if (iNPUT_EVENT == POINTER_INFO.INPUT_EVENT.PRESS)
			{
				_lastPressedButton = button;
			}
			if (iNPUT_EVENT == inputEvent && _lastPressedButton == button && Loading.IsReady && InputEnabled)
			{
				if (playSound)
				{
					SoundManager.Play("UI/generic_button_tap", false);
				}
				actionDelegate();
			}
		};
		if (addDelegate)
		{
			button.AddInputDelegate(eZInputDelegate);
		}
		else
		{
			button.SetInputDelegate(eZInputDelegate);
		}
	}

	public static string GetScreenName(Type screenType)
	{
		return screenType.ToString().Replace("Screen", string.Empty);
	}

	public static string GetScreenName<TScreen>()
	{
		return GetScreenName(typeof(TScreen));
	}

	public static GameObject SpawnChildPrefab(Transform parent, GameObject prototype)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prototype) as GameObject;
		gameObject.transform.parent = parent;
		gameObject.transform.localPosition = Vector3.zero;
		return gameObject;
	}

	public static Vector3 FindSizeOfGameObjectUsingSpriteRoot(GameObject go)
	{
		Vector3 vector = Vector3.zero;
		foreach (SpriteRoot spriteRoot in go.transform.GetComponentsInChildren<SpriteRoot>(true))
		{
			vector = Vector3.Max(new Vector3(spriteRoot.width, spriteRoot.height, 0f), vector);
		}
		return vector;
	}

	public static void BlockButton(UIButton button, bool block)
	{
		UIButtonBlocker uIButtonBlocker = button.gameObject.GetComponent<UIButtonBlocker>();
		if (block)
		{
			if (uIButtonBlocker == null)
			{
				uIButtonBlocker = button.gameObject.AddComponent<UIButtonBlocker>();
			}
			uIButtonBlocker.SetBlock(true);
		}
		else
		{
			UnityEngine.Object.Destroy(uIButtonBlocker);
		}
	}

	public static void BlinkSprite(SpriteRoot sprite, bool blink, Color color)
	{
		UISpriteBlinker uISpriteBlinker = sprite.gameObject.GetComponent<UISpriteBlinker>();
		if (blink)
		{
			if (uISpriteBlinker == null)
			{
				uISpriteBlinker = sprite.gameObject.AddComponent<UISpriteBlinker>();
			}
			uISpriteBlinker.BlinkColor = color;
		}
		else
		{
			UnityEngine.Object.Destroy(uISpriteBlinker);
		}
	}

	public static void FinishAllAnimations(GameObject go)
	{
		Array.ForEach(go.GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
		{
			c.Finish();
		});
	}

	public static void PrintAllAnimations(GameObject go)
	{
		Array.ForEach(go.GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
		{
			Debug.Log(go.name + " - " + c, go);
		});
	}

	public static void PauseAllAnimations(GameObject go)
	{
		Array.ForEach(go.GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
		{
			c.enabled = false;
		});
	}

	public static void ResumeAllAnimations(GameObject go)
	{
		Array.ForEach(go.GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
		{
			c.enabled = true;
		});
	}

	public static float RunAllAnimations(GameObject go)
	{
		float totalDuration = 0f;
		Array.ForEach(go.GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
		{
			totalDuration = Mathf.Max(totalDuration, c.Delay + c.Duration);
			c.RunAnimation();
		});
		return totalDuration;
	}

	public static void DelayButtonFunctionality(UIButton button, float delay)
	{
		button.gameObject.AddComponent<RTMiniScript>().Charge(delay, null, (MiniScript s) =>
		{
			button.controlIsEnabled = false;
		}, null, (MiniScript s) =>
		{
			button.controlIsEnabled = true;
		});
	}

	public static void SetupUIHighlightShader(Material material, float multiplier, float duration, float blinkDuration, Color color)
	{
		material.SetFloat("_Multiplier", multiplier);
		material.SetFloat("_Duration", duration);
		material.SetFloat("_BlinkDuration", blinkDuration);
		material.SetColor("_Color", color);
	}

	public static Color NewColorWithAlpha(Color c, float alpha)
	{
		return new Color(c.r, c.g, c.b, alpha);
	}

	public static void SetupPriceText(SpriteText text, int price, string currency)
	{
		text.Text = GetPriceStringForCurrency(price, currency);
	}

	public static string GetPriceStringForCurrency(int price, string currency)
	{
		return currency == "soft" ? FormatSoftCurrencyString(price) : FormatHardCurrencyString(price);
	}

	public static string GetSymbolForCurrency(string currency)
	{
		return currency == "soft" ? SoftCurrencySymbol : HardCurrencySymbol;
	}

	public static Color GetColorForCurrency(string currency)
	{
		return currency == "soft" ? Color.white : ColorYellowHard;
	}

	public static string FormatHardCurrencyString(int count)
	{
		return FormatCurrencyString(count, HardCurrencySymbol, ColorYellowHard);
	}

	public static string FormatSoftCurrencyString(int count)
	{
		return FormatCurrencyString(count, SoftCurrencySymbol, null);
	}

	public static string FormatCurrencyString(int count, string symbol, Color? color)
	{
		return string.Format("{0}{1}{2:#,###0}", symbol, !(!color.HasValue) ? color.ToString() : string.Empty, count);
	}
}
