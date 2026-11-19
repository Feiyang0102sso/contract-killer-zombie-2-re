using UnityEngine;

public class UITextAutoScale : MonoBehaviour
{
	public float MaxWidth = 1000f;

	public float MaxHeight = 1000f;

	private SpriteText _spriteText;

	private void Awake()
	{
		_spriteText = GetComponent<SpriteText>();
	}

	private void Update()
	{
		float a = 1f;
		a = Mathf.Min(a, MaxWidth / _spriteText.TotalWidth);
		a = Mathf.Min(a, MaxHeight / _spriteText.BaseHeight);
		if (a < 1f)
		{
			_spriteText.SetCharacterSize(_spriteText.characterSize * a);
		}
		Object.Destroy(this);
	}
}
