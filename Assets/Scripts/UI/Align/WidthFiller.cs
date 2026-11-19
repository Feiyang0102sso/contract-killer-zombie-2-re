using UnityEngine;

public class WidthFiller : MonoBehaviour
{
	public bool Tile;

	public float TileWidth;

	public float TilingY = 1f;

	private void Start()
	{
		FillWidth();
	}

	private void FillWidth()
	{
		if (Tile && TileWidth > 0f)
		{
			Renderer component = GetComponent<Renderer>();
			if ((bool)component && (bool)component.material && (bool)component.material.mainTexture)
			{
				component.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
				component.material.mainTextureScale = new Vector2(GUIUtils.ScreenWidth() / TileWidth, TilingY);
			}
		}
		SpriteRoot component2 = gameObject.GetComponent<SpriteRoot>();
		component2.SetSize(GUIUtils.ScreenWidth(), component2.height);
	}
}
