// New Unity port: replaces Graphics.Blit(source, null, material[, pass]) with its Unity 3.5 meaning, "draw to the screen".
// With a post effect on, the game's PostEffectManager renders the main camera into an RT, then in OnPostRender calls
// Blit(rt, null, mat) to output it to the screen.
// In newer Unity a null target means: if Camera.main has a targetTexture, write back into that RT instead of the screen,
// so the RT is written onto itself and the back buffer stays empty (measured: the whole screen white). This sets the back
// buffer as the target and draws a full-screen quad with GL, the way Graphics.Blit does it
// (_MainTex = source, orthographic projection, every pass in turn when pass < 0).
using UnityEngine;

public static class NewUnityBlit
{
	/// <summary>Plain copy shader used when material is null (the one Graphics.Blit itself uses).</summary>
	private const string CopyShaderName = "Hidden/BlitCopy";

	private static Material _copyMaterial;

	/// <summary>Draws source to the screen through material (null = plain copy). pass below 0 means every pass.</summary>
	public static void ToScreen(RenderTexture source, Material material, int pass = -1)
	{
		Material drawMaterial = material;
		if (drawMaterial == null)
		{
			drawMaterial = GetCopyMaterial();
		}
		Graphics.SetRenderTarget((RenderTexture)null);
		drawMaterial.SetTexture("_MainTex", source);
		int firstPass = pass;
		int lastPass = pass;
		if (pass < 0)
		{
			firstPass = 0;
			lastPass = drawMaterial.passCount - 1;
		}
		GL.PushMatrix();
		GL.LoadOrtho();
		for (int index = firstPass; index <= lastPass; index++)
		{
			drawMaterial.SetPass(index);
			GL.Begin(GL.QUADS);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(1f, 0f, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(1f, 1f, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(0f, 1f, 0f);
			GL.End();
		}
		GL.PopMatrix();
	}

	private static Material GetCopyMaterial()
	{
		if (_copyMaterial == null)
		{
			Shader shader = Shader.Find(CopyShaderName);
			if (shader == null)
			{
				Debug.LogError("[NewUnityBlit] shader not found: " + CopyShaderName);
			}
			_copyMaterial = new Material(shader);
		}
		return _copyMaterial;
	}
}
