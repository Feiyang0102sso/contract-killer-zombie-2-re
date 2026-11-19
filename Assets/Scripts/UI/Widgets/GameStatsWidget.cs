using UnityEngine;

public class GameStatsWidget : MonoBehaviour
{
	public SpriteText StatText;

	public SpriteText StatVal;

	public SpriteText StatBonus;

	private int _statCount;

	private float _spacingY = 9f;

	public void SetSpacingY(float spacingY)
	{
		_spacingY = spacingY;
	}

	public Transform LastAddedStatGroup { get; private set; }

	public void AddStat(string str, string val)
	{
		AddStat(str, val, string.Empty, false, 0);
	}

	public void AddStat(string str, int val)
	{
		AddStat(str, string.Empty, string.Empty, true, val);
	}

	public void AddStat(string str, int val, string bonus)
	{
		AddStat(str, string.Empty, bonus, true, val);
	}

	public void AddStat(string str, string val, string bonus)
	{
		AddStat(str, val, bonus, false, 0);
	}

	public void AddStat(string str, string val, string bonus, bool countAnimation, int valInt)
	{
		SpriteText statText = StatText;
		SpriteText statVal = StatVal;
		SpriteText spriteText = StatBonus;
		if (_statCount == 0)
		{
			StatText.gameObject.active = false;
			StatVal.gameObject.active = false;
			if (StatBonus != null)
			{
				StatBonus.gameObject.active = false;
			}
		}
		Transform transform = new GameObject("Stat" + _statCount).transform;
		float unusedStatValue1;
		float unusedStatValue2;
		transform.parent = StatText.gameObject.GetParent().transform.parent;
		transform.localPosition = StatText.gameObject.GetParent().transform.localPosition;
		transform.Translate(0f, (float)(-_statCount) * _spacingY, 0f);
		statText = (Object.Instantiate(StatText.gameObject) as GameObject).GetComponent<SpriteText>();
		statText.isClone = true;
		statText.transform.parent = transform;
		statText.transform.localPosition = StatText.transform.localPosition;
		statVal = (Object.Instantiate(StatVal.gameObject) as GameObject).GetComponent<SpriteText>();
		statVal.isClone = true;
		statVal.transform.parent = transform;
		statVal.transform.localPosition = StatVal.transform.localPosition;
		if (StatBonus != null)
		{
			spriteText = (Object.Instantiate(StatBonus.gameObject) as GameObject).GetComponent<SpriteText>();
			spriteText.isClone = true;
			spriteText.transform.parent = transform;
			spriteText.transform.localPosition = StatBonus.transform.localPosition;
		}
		statText.Text = str;
		statVal.Text = val;
		if (StatBonus != null)
		{
			if (bonus != string.Empty)
			{
				spriteText.Text = bonus;
			}
			else
			{
				spriteText.gameObject.active = false;
			}
		}
		statText.gameObject.AddComponent<UIAnimationTypingText>().Setup((float)_statCount * 0.65f, 0.65f, EZAnimation.EASING_TYPE.Linear);
		statVal.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, (float)(_statCount + 1) * 0.65f, 0.4f, EZAnimation.EASING_TYPE.ExponentialIn);
		spriteText.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, (float)(_statCount + 1) * 0.65f, 0.4f, EZAnimation.EASING_TYPE.ExponentialIn);
		if (countAnimation)
		{
			statVal.gameObject.AddComponent<UIAnimationCountText>().Setup(0, valInt, (float)(_statCount + 1) * 0.65f + 0.2f, 1f, EZAnimation.EASING_TYPE.Linear);
		}
		LastAddedStatGroup = transform;
		_statCount++;
	}
}
