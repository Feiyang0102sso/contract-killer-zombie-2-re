using UnityEngine;

public class EasySnapScrollList : UIScrollListLooper
{
	public const float ScrollDamping = 10f;

	private bool _scrollingBegin;

	private bool _scrollingFinished = true;

	private float _absoluteScrollPosBeforeScrolling;

	private float _targetedPos;

	private bool _isAutoScrolling;

	protected override void Update()
	{
		if (!isScrolling && !_scrollingFinished && items.Count > 0)
		{
			_scrollingFinished = true;
			_scrollingBegin = false;
			int num = (int)Mathf.Sign(AbsouluteScrollPosition - _absoluteScrollPosBeforeScrolling);
			float num2 = 1f / (float)(items.Count - 1);
			_targetedPos = (float)(Mathf.RoundToInt(_absoluteScrollPosBeforeScrolling / num2) + num) * num2;
			_isAutoScrolling = true;
		}
		else if (isScrolling && !_scrollingBegin)
		{
			_scrollingBegin = true;
			_scrollingFinished = false;
		}
		else if (isScrolling)
		{
			float num3 = 1f / (float)(items.Count - 1) * 0.9f;
			if (AbsouluteScrollPosition > _absoluteScrollPosBeforeScrolling + num3)
			{
				AbsouluteScrollPosition = _absoluteScrollPosBeforeScrolling + num3;
			}
			else if (AbsouluteScrollPosition < _absoluteScrollPosBeforeScrolling - num3)
			{
				AbsouluteScrollPosition = _absoluteScrollPosBeforeScrolling - num3;
			}
		}
		else
		{
			_absoluteScrollPosBeforeScrolling = AbsouluteScrollPosition;
		}
		if (!isScrolling && _isAutoScrolling)
		{
			AbsouluteScrollPosition = Mathf.Lerp(AbsouluteScrollPosition, _targetedPos, 10f * Time.deltaTime);
		}
		if (Mathf.Abs(AbsouluteScrollPosition - _targetedPos) < 0.001f)
		{
			_isAutoScrolling = false;
		}
		base.Update();
	}

	public override void SetCurrentItem(object data)
	{
		_isAutoScrolling = false;
		base.SetCurrentItem(data);
	}
}
