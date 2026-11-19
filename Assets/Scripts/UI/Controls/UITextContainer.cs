using System;
using UnityEngine;

public class UITextContainer : MonoBehaviour
{
	private const float SpacingX = 2f;

	public void Relayout()
	{
		SpriteText[] componentsInChildren = GetComponentsInChildren<SpriteText>();
		Array.Sort(componentsInChildren, (SpriteText a, SpriteText b) => Util.CompareNatural(a.gameObject.name, b.gameObject.name));
		float num = 0f;
		foreach (SpriteText spriteText in componentsInChildren)
		{
			num += spriteText.TotalWidth + 2f;
		}
		float num3 = (-num) * 0.5f;
		foreach (SpriteText spriteText2 in componentsInChildren)
		{
			num3 += spriteText2.TotalWidth * 0.5f;
			spriteText2.transform.localPosition = new Vector3(num3, 0f, 0f);
			num3 += spriteText2.TotalWidth * 0.5f + 2f;
		}
	}
}
