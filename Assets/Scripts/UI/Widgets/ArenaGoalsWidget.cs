using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArenaGoalsWidget : MonoBehaviour
{
	public GameObject ArenaGoalPanelPrefab;

	private UIScrollList _list;

	private List<ArenaGoalPanel> _items = new List<ArenaGoalPanel>();

	private List<string> _newGoals = new List<string>();

	private float ScaleFrom = 1f;

	private float ScaleTo = 1f;

	private float ScaleFactor = 3.5f;

	private float AlphaFrom = 1f;

	private float AlphaTo;

	private float AlphaFactor = 5f;

	private bool _animated;

	private void Awake()
	{
		_list = GetComponentInChildren<UIScrollList>();
	}

	public void Init()
	{
		FillItems();
	}

	public void FillItems()
	{
		_items.Clear();
		_list.ClearList(true);
		_list.positionItemsImmediately = true;
		int num = 0;
		foreach (KeyValuePair<string, PlayerData.ArenaGoalInfo> item in Singleton.PlayerData.ArenaGoals.Concat(Singleton.PlayerData.LastCompletedArenaGoals))
		{
			string key = item.Key;
			PlayerData.ArenaGoalInfo info = Singleton.PlayerData.ArenaGoals.Concat(Singleton.PlayerData.LastCompletedArenaGoals).ToDictionary((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => p.Key, (KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => p.Value)[key];
			bool isCompleted = false;
			bool isObsolete = false;
			if (ArenaWaves.IsObsolete(key))
			{
				isObsolete = true;
			}
			if (Singleton.PlayerData.LastCompletedArenaGoals.ContainsKey(key))
			{
				isCompleted = true;
			}
			AddGoalItem(key, info, isCompleted, isObsolete);
		}
		_list.ScrollPosition = 0.4f;
		_newGoals = ArenaWaves.RefreshGoals().Keys.ToList();
		UpdateItemsScaleEffect(true);
		StartCoroutine(PlayNextAnimation());
	}

	private IEnumerator PlayNextAnimation()
	{
		float unusedLocal1;
		float unusedLocal2;
		float unusedLocal3;
		_animated = true;
		_list.controlIsEnabled = false;
		int lastCompletedGoalIndex = -1;
		while (true)
		{
			Vector3 shiftPos = Vector3.zero;
			ArenaGoalPanel completedGoal = null;
			float delay = 0.1f;
			float totalDuration = 0f;
			int index = 0;
			foreach (ArenaGoalPanel panel in _items)
			{
				GameObject element = panel.transform.parent.gameObject;
				Vector3 localPos = element.transform.localPosition;
				if (panel.IsCompleted || panel.IsObsolete)
				{
					if (completedGoal == null && index > lastCompletedGoalIndex)
					{
						if (panel.IsObsolete)
						{
							panel.CompletedText.Color = GUIUtils.NewColorWithAlpha(panel.CompletedText.Color, 0f);
						}
						else
						{
							panel.CompletedText.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, delay, 0.25f, EZAnimation.EASING_TYPE.Linear);
							panel.CompletedText.gameObject.AddComponent<UIAnimationScale>().Setup(4.5f, 1f, delay, 0.5f, EZAnimation.EASING_TYPE.BackIn);
							panel.CompletedText.gameObject.AddComponent<UIAnimationRotate>().Setup(0f, 14f, new Vector3(0f, 0f, 1f), delay, 0.5f, EZAnimation.EASING_TYPE.ExponentialIn);
						}
						element.AddComponent<UIAnimationDeltaMove>().Setup(-200f, 0f, 0f, 1.5f + delay, 0.5f, EZAnimation.EASING_TYPE.BackIn);
						shiftPos = localPos;
						completedGoal = panel;
						lastCompletedGoalIndex = index;
						delay += 2f;
					}
					else
					{
						panel.CompletedText.Color = GUIUtils.NewColorWithAlpha(panel.CompletedText.Color, 0f);
					}
				}
				if (completedGoal != null && index > lastCompletedGoalIndex)
				{
					Vector3 delta = shiftPos - localPos;
					shiftPos = localPos;
					element.AddComponent<UIAnimationDeltaMove>().Setup(delta.x, delta.y, delta.z, delay, 0.5f, EZAnimation.EASING_TYPE.ExponentialOut);
					delay += 0.3f;
				}
				totalDuration = Mathf.Max(totalDuration, GUIUtils.RunAllAnimations(element));
				index++;
			}
			if (completedGoal == null)
			{
				break;
			}
			float t = TimeControl.RealTime + totalDuration;
			while (t > TimeControl.RealTime)
			{
				yield return new WaitForEndOfFrame();
			}
			RemoveGoalItem(completedGoal);
			lastCompletedGoalIndex--;
		}
		foreach (string goal in _newGoals)
		{
			float totalDuration = 0f;
			PlayerData.ArenaGoalInfo info = Singleton.PlayerData.ArenaGoals.Concat(Singleton.PlayerData.LastCompletedArenaGoals).ToDictionary((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => p.Key, (KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => p.Value)[goal];
			ArenaGoalPanel panel = AddGoalItem(goal, info, false, false);
			panel.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 0f, 0.5f, EZAnimation.EASING_TYPE.Linear);
			panel.gameObject.AddComponent<UIAnimationScale>().Setup(4f, 1f, 0f, 1f, EZAnimation.EASING_TYPE.ExponentialOut);
			totalDuration = Mathf.Max(totalDuration, GUIUtils.RunAllAnimations(panel.gameObject));
			float t = TimeControl.RealTime + totalDuration;
			while (t > TimeControl.RealTime)
			{
				yield return new WaitForEndOfFrame();
			}
		}
		_list.controlIsEnabled = true;
		_animated = false;
	}

	private ArenaGoalPanel AddGoalItem(string goal, PlayerData.ArenaGoalInfo info, bool isCompleted, bool isObsolete)
	{
		ArenaGoalPanel component = (Object.Instantiate(ArenaGoalPanelPrefab) as GameObject).GetComponent<ArenaGoalPanel>();
		component.Init(goal, info, isCompleted, isObsolete);
		AddItem(component.gameObject);
		_items.Add(component);
		return component;
	}

	private void RemoveGoalItem(ArenaGoalPanel panel)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			if (_list.GetItem(i).transform == panel.transform.parent)
			{
				_list.RemoveItem(i, true);
				break;
			}
		}
		_items.Remove(panel);
	}

	private void AddItem(GameObject plate)
	{
		GameObject gameObject = new GameObject("element");
		UIListItemContainer uIListItemContainer = gameObject.AddComponent<UIListItemContainer>();
		plate.transform.parent = uIListItemContainer.transform;
		plate.transform.localPosition = Vector3.zero;
		uIListItemContainer.FindOuterEdges();
		Vector2 bottomRightEdge = uIListItemContainer.BottomRightEdge;
		_list.InsertItem(uIListItemContainer.gameObject.GetComponent(typeof(IUIListObject)) as IUIListObject, _list.Count);
		uIListItemContainer.ScanChildren();
	}

	private void Update()
	{
		if (!_animated)
		{
			UpdateItemsScaleEffect(false);
		}
	}

	private void UpdateItemsScaleEffect(bool all)
	{
		foreach (ArenaGoalPanel item in _items)
		{
			if (!(item.gameObject.active || all))
			{
				continue;
			}
			Transform transform = item.transform;
			float num = Mathf.Lerp(ScaleFrom, ScaleTo, Mathf.Pow(Mathf.Abs(transform.localPosition.y / (_list.viewableArea.y * 0.5f)), ScaleFactor));
			transform.localScale = new Vector3(num, num, num);
			float alpha = Mathf.Lerp(AlphaFrom, AlphaTo, Mathf.Pow(Mathf.Abs(transform.localPosition.y / (_list.viewableArea.y * 0.5f)), AlphaFactor));
			item.SetAlpha(alpha);
		}
	}
}
