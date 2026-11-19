using Glu.Localization;
using UnityEngine;

public class MapLocation : MonoBehaviour
{
	private const float AnimDelay = 0.33f;

	private const float AnimDuration = 1.5f;

	private const float BlinkingDuration = 1f;

	private const int BlinkingCycles = 2;

	public int Level = 1;

	public GameData.MissionLocation Location;

	public Vector3 IconOffset = Vector3.zero;

	private MapNarrative _mapNarrative;

	private GameObject _button;

	private UIButton _uiButton;

	private UIPressableButton _uiPressableButton;

	private Color _cachedMissionColor;

	private MeshFilter _filter;

	private bool _specialHuntLocation;

	private int _prevMissionIndex = -1;

	private bool _enableAnimations = true;

	private bool _lastCompleted;

	private int _lastCompletedMission = -1;

	private int _lastCompletedMissionSeed = -1;

	private bool _lastCompletedMissionIsSpecialHunt;

	public static Color Disabled = new Color(1f, 1f, 1f, 0f);

	public static Color Normal = new Color(0f, 1f, 1f / 255f, 1f);

	public static Color Hard = new Color(1f, 241f / 255f, 1f / 255f, 1f);

	public static Color Storyline = new Color(1f / 255f, 254f / 255f, 251f / 255f, 1f);

	public static Color Locked = new Color(128f / 255f, 128f / 255f, 128f / 255f, 1f);

	public static Color Survival = new Color(1f, 171f / 255f, 0f, 1f);

	public static Color SpecialHunt = Survival;

	public static int AnimIndex = 0;

	public GameObject Button
	{
		get
		{
			return _button;
		}
	}

	private void Awake()
	{
		_mapNarrative = gameObject.FindComponentInParents<MapNarrative>();
		_filter = gameObject.GetComponent<MeshFilter>();
		PlotUpdate(false);
	}

	private Color GetMissionColor(int missionIndex, int generationSeed)
	{
		if (missionIndex >= 0 && missionIndex <= Singleton.GameData.Missions.Length)
		{
			Mission.Type type = Singleton.GameData.Missions[missionIndex].Type;
			if (Mission.IsSurvival(type))
			{
				return Survival;
			}
			if (Mission.IsStorylineMission(type))
			{
				if (!_lastCompleted && Singleton.PlayerData.StorylineProgress < Singleton.GameData.Acts[Singleton.PlayerData.Act].Required)
				{
					return Locked;
				}
				return Storyline;
			}
			if (!Singleton.PlayerData.IsSpecialHuntReceived && missionIndex == Singleton.ShopData.SpecialHunt.missionIndex)
			{
				return SpecialHunt;
			}
			switch (Singleton.PlayerData.GetMissionDifficulty(missionIndex, generationSeed))
			{
			case Mission.Difficulty.Normal:
				return Normal;
			case Mission.Difficulty.Hard:
				return Hard;
			default:
				return Color.white;
			}
		}
		return Disabled;
	}

	public Color MissionColor
	{
		get
		{
			if (HasMission)
			{
				int mission = Singleton.PlayerData.GetMission(gameObject.name);
				int missionGenerationSeed = Singleton.PlayerData.GetMissionGenerationSeed(gameObject.name);
				return GetMissionColor(mission, missionGenerationSeed);
			}
			if (_lastCompleted)
			{
				return GetMissionColor(_lastCompletedMission, _lastCompletedMissionSeed);
			}
			return Disabled;
		}
	}

	public string MissionDifficultyText
	{
		get
		{
			if (HasMission)
			{
				if (Mission.IsStorylineMission(MissionData.Type))
				{
					return null;
				}
				switch (MissionDifficulty)
				{
				case Mission.Difficulty.Normal:
					return null;
				case Mission.Difficulty.Hard:
					return Strings.GetString("HARD!");
				default:
					return null;
				}
			}
			return null;
		}
	}

	private void AnimateButtonDisappearance(GameObject button)
	{
		UIAnimationScale uIAnimationScale = button.AddComponent<UIAnimationScale>();
		uIAnimationScale.Duration = 1.5f;
		uIAnimationScale.Delay = 0.33f * (float)AnimIndex;
		uIAnimationScale.From = 1f;
		uIAnimationScale.To = 0f;
		uIAnimationScale.DestroyObjectOnFinish = true;
		uIAnimationScale.InterpolatorType = EZAnimation.EASING_TYPE.ExponentialIn;
		UIAnimationAlpha uIAnimationAlpha = button.AddComponent<UIAnimationAlpha>();
		uIAnimationAlpha.Duration = 1.5f;
		uIAnimationAlpha.Delay = 0.33f * (float)AnimIndex;
		uIAnimationAlpha.From = 1f;
		uIAnimationAlpha.To = 0f;
		uIAnimationAlpha.DestroyObjectOnFinish = true;
		uIAnimationAlpha.InterpolatorType = EZAnimation.EASING_TYPE.ExponentialIn;
		GUIUtils.RunAllAnimations(button);
		GUIUtils.PauseAllAnimations(button);
		AnimIndex++;
	}

	private void AnimateButtonAppearance(GameObject button)
	{
		UIAnimationMove uIAnimationMove = button.AddComponent<UIAnimationMove>();
		uIAnimationMove.Duration = 1.5f;
		uIAnimationMove.Delay = 0.33f * (float)AnimIndex;
		uIAnimationMove.DeltaY = 250f;
		uIAnimationMove.InterpolatorType = EZAnimation.EASING_TYPE.ExponentialOut;
		UIAnimationScale uIAnimationScale = button.AddComponent<UIAnimationScale>();
		uIAnimationScale.Duration = 1.5f;
		uIAnimationScale.Delay = 0.33f * (float)AnimIndex;
		uIAnimationScale.From = 10f;
		uIAnimationScale.To = 1f;
		uIAnimationScale.InterpolatorType = EZAnimation.EASING_TYPE.ExponentialOut;
		UIAnimationAlpha uIAnimationAlpha = button.AddComponent<UIAnimationAlpha>();
		uIAnimationAlpha.Duration = 1.5f;
		uIAnimationAlpha.Delay = 0.33f * (float)AnimIndex;
		uIAnimationAlpha.From = 0f;
		uIAnimationAlpha.To = 1f;
		uIAnimationAlpha.InterpolatorType = EZAnimation.EASING_TYPE.ExponentialOut;
		GUIUtils.RunAllAnimations(button);
		GUIUtils.PauseAllAnimations(button);
		AnimIndex++;
	}

	private void AnimateButtonBlinking(GameObject button)
	{
		for (int i = 0; i < 2; i++)
		{
			UIAnimationScale uIAnimationScale = button.AddComponent<UIAnimationScale>();
			uIAnimationScale.Duration = 0.5f;
			uIAnimationScale.Delay = (float)i * 1f / 2f;
			uIAnimationScale.From = (!(i % 2 != 0) ? 1f : 1.5f);
			uIAnimationScale.To = (!(i % 2 != 0) ? 1.5f : 1f);
			uIAnimationScale.InterpolatorType = EZAnimation.EASING_TYPE.SinusoidalInOut;
		}
		GUIUtils.RunAllAnimations(button);
		GUIUtils.PauseAllAnimations(button);
	}

	public void PlotUpdate(bool animate)
	{
		animate = animate && _enableAnimations;
		PackedSprite component = base.gameObject.GetComponent<PackedSprite>();
		component.Color = HasMission ? Color.white : new Color(1f, 1f, 1f, 0f);
		if (HasMission)
		{
			int mission = PlayerData.Instance.GetMission(base.gameObject.name);
			animate = animate && _prevMissionIndex != mission;
			_prevMissionIndex = mission;
			if (Mission.IsSurvival(Singleton.GameData.Missions[mission].Type))
			{
				_enableAnimations = false;
			}
		}
		else
		{
			_prevMissionIndex = -1;
		}
		if ((bool)_button)
		{
			if ((bool)_button.GetComponent<Collider>())
			{
				Object.Destroy(_button.GetComponent<Collider>());
			}
			if (animate)
			{
				Debug.Log("Disappearing " + base.gameObject.name);
				AnimateButtonDisappearance(_button.gameObject);
			}
			else
			{
				Object.Destroy(_button.gameObject);
			}
			_button = null;
			_uiButton = null;
			_uiPressableButton = null;
		}
		if (HasMission)
		{
			_lastCompleted = false;
			_lastCompletedMission = -1;
			_lastCompletedMissionSeed = -1;
			_lastCompletedMissionIsSpecialHunt = false;
			int mission2 = PlayerData.Instance.GetMission(base.gameObject.name);
			GameObject missionButton = _mapNarrative.GetMissionButton(mission2);
			_specialHuntLocation = (bool)missionButton && missionButton == _mapNarrative.SpecialHuntButton;
			_button = Object.Instantiate(missionButton, transform.position, transform.rotation) as GameObject;
			_button.transform.parent = transform;
			_button.transform.position += IconOffset;
			_button.transform.position += new Vector3(0f, 0f, missionButton.transform.position.z - _button.transform.position.z);
			_button.transform.position += new Vector3(0f, _button.GetComponent<UIButton>().height / 2f, 0f);
			_button.active = true;
			_button.name = MissionData.Descriptor;
			_uiButton = _button.GetComponent<UIButton>();
			_uiPressableButton = _button.GetComponent<UIPressableButton>();
			_uiButton.Color = MissionColor;
			_uiPressableButton.SetBaseColor(MissionColor);
			_uiPressableButton.OnPressColor = MissionColor;
			bool actRequirementMet = Singleton.PlayerData.StorylineProgress == Singleton.GameData.Acts[Singleton.PlayerData.Act].Required;
			if (actRequirementMet && Mission.IsStorylineMission(Singleton.GameData.Missions[mission2].Type))
			{
				Debug.Log("Blinking " + base.gameObject.name);
				AnimateButtonBlinking(_button.gameObject);
			}
			else if (animate)
			{
				Debug.Log("Appearing " + base.gameObject.name);
				AnimateButtonAppearance(_button.gameObject);
			}
		}
		else if (PlayerData.Instance.LastCompletedMissionLocation == base.gameObject.name)
		{
			_lastCompleted = true;
			_lastCompletedMission = PlayerData.Instance.LastCompletedMission;
			_lastCompletedMissionSeed = PlayerData.Instance.LastCompletedMissionSeed;
			_lastCompletedMissionIsSpecialHunt = PlayerData.Instance.LastCompletedMissionIsSpecialHunt;
			GameObject gameObject = _mapNarrative.GetMissionButton(_lastCompletedMission);
			if (_lastCompletedMissionIsSpecialHunt)
			{
				gameObject = _mapNarrative.SpecialHuntButton;
			}
			_button = Object.Instantiate(gameObject, transform.position, transform.rotation) as GameObject;
			_button.transform.parent = transform;
			_button.transform.position += new Vector3(0f, 0f, gameObject.transform.position.z - _button.transform.position.z);
			_button.transform.position += new Vector3(0f, _button.GetComponent<UIButton>().height / 2f, 0f);
			_button.active = true;
			_button.name = "LastCompletedMission";
			if ((bool)_button.GetComponent<Collider>())
			{
				Object.Destroy(_button.GetComponent<Collider>());
			}
			_uiButton = _button.GetComponent<UIButton>();
			_uiPressableButton = _button.GetComponent<UIPressableButton>();
			_uiButton.Color = MissionColor;
			_uiPressableButton.SetBaseColor(MissionColor);
			_uiPressableButton.OnPressColor = MissionColor;
			AnimateButtonDisappearance(_button.gameObject);
			PlayerData.Instance.CleanupLastCompletedMissionInfo();
			_prevMissionIndex = -1;
			_button = null;
			_uiButton = null;
			_uiPressableButton = null;
		}
	}

	public bool HasMission
	{
		get
		{
			return PlayerData.Instance.HasMission(gameObject.name);
		}
	}

	public int MissionIndex
	{
		get
		{
			return HasMission ? PlayerData.Instance.GetMission(gameObject.name) : (-1);
		}
	}

	public Mission.Difficulty MissionDifficulty
	{
		get
		{
			return HasMission ? PlayerData.Instance.GetMissionDifficulty(gameObject.name) : Mission.Difficulty.Normal;
		}
	}

	public GameData.MissionData MissionData
	{
		get
		{
			return HasMission ? Singleton.GameData.Missions[MissionIndex] : null;
		}
	}

	public void SelectMission()
	{
		int mission = PlayerData.Instance.GetMission(gameObject.name);
		_mapNarrative.ShowMissionDialogBox(this, mission);
	}

	private void SpecialHuntStopped()
	{
		if (Singleton.PlayerData.HasMission(gameObject.name) && Singleton.PlayerData.GetMission(gameObject.name) == Singleton.ShopData.SpecialHunt.missionIndex)
		{
			Singleton.PlayerData.RemoveMission(gameObject.name);
			Object.Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (Util.DoRarely(this, "update_colors", 0.5f) && (bool)_uiButton && (bool)_uiPressableButton && _cachedMissionColor != MissionColor)
		{
			_uiButton.Color = MissionColor;
			_uiPressableButton.SetBaseColor(MissionColor);
			_uiPressableButton.OnPressColor = MissionColor;
			_cachedMissionColor = MissionColor;
		}
	}
}
