// PC 桥接：替代 Unity 3.5 语义的 Graphics.Blit(source, null, material[, pass])，即“画到屏幕”。
// 游戏的 PostEffectManager 开后处理时把主相机渲染到一张 RT，再在 OnPostRender 里 Blit(rt, null, mat) 输出到屏幕。
// 新版 Unity 规定：目标为 null 时，如果 Camera.main 有 targetTexture，就写回那张 RT 而不是屏幕，
// 结果是 RT 写自己、后备缓冲区没有画面（实测整屏白色）。这里直接把后备缓冲区设为目标，用 GL 画一个全屏四边形，
// 与 Graphics.Blit 的做法相同（_MainTex = source，正交投影，pass < 0 时依次画所有 pass）。
using UnityEngine;

public static class PcBlit
{
	/// <summary>material 为 null 时用的纯拷贝着色器（Graphics.Blit 自己用的那个）。</summary>
	private const string CopyShaderName = "Hidden/BlitCopy";

	private static Material _copyMaterial;

	/// <summary>把 source 经 material（null 表示直接拷贝）画到屏幕。pass 小于 0 表示所有 pass。</summary>
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
				Debug.LogError("[PcBlit] shader not found: " + CopyShaderName);
			}
			_copyMaterial = new Material(shader);
		}
		return _copyMaterial;
	}
}
