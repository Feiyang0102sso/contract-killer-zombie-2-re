using UnityEngine;

public class TripleProgressBar : MonoBehaviour
{
	private const float MinNegativeAddVal = -0.08f;

	public UIBorderSprite Back;

	public UIBorderSprite Frame;

	public UIBorderSprite ArrowGrey;

	public UIBorderSprite ArrowGreen;

	public UIBorderSprite ArrowRed;

	public float Width = 31f;

	public float BorderLeft;

	public float BorderRight;

	private float _baseVal = 0.5f;

	private float _addVal;

	public float Value
	{
		get
		{
			return _baseVal;
		}
		set
		{
			_baseVal = Mathf.Min(value, 1f);
			UpdateSizes();
		}
	}

	public float AdditionalValue
	{
		get
		{
			return _addVal;
		}
		set
		{
			_addVal = value;
			_addVal = Mathf.Clamp(_addVal, -Value, 1f - Value);
			if (_addVal < 0f && _addVal > -0.08f)
			{
				_addVal = -0.08f;
			}
			UpdateSizes();
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
		UpdateSizes();
	}

	private void UpdateSizes()
	{
		Back.SetWidth(Width);
		Back.transform.localPosition = Vector3.zero;
		Frame.SetWidth(Width);
		Frame.transform.localPosition = new Vector3(0f, 0f, -1.5f);
		float num = -0.5f * Width + BorderLeft;
		float num2 = Back.Width - (BorderLeft + BorderRight);
		float num3 = num2 * _baseVal;
		float num4 = num2 * _addVal;
		ArrowGrey.SetWidth(num3 + num4 / 2f);
		ArrowGrey.transform.localPosition = new Vector3(num + ArrowGrey.Width * 0.5f, 0f, -0.5f);
		float num5 = ArrowGrey.BorderRight * 0.5f;
		ArrowGreen.gameObject.SetActiveRecursively(_addVal > 0f);
		ArrowGreen.SetInternalWidth((num2 - ArrowGreen.BorderRight - ArrowGreen.BorderLeft - num5) * _addVal);
		ArrowGreen.transform.localPosition = new Vector3(num - ArrowGreen.BorderLeft + num3 + num5 + ArrowGreen.Width * 0.5f, 0f, -1f);
		ArrowRed.gameObject.SetActiveRecursively(_addVal < 0f);
		ArrowRed.SetInternalWidth(-num4 - ArrowRed.BorderRight);
		ArrowRed.transform.localPosition = new Vector3(num + num3 - ArrowRed.Width * 0.5f, 0f, -1f);
	}
}
