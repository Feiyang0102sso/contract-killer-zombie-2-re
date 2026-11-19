using System;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
	public enum Mode
	{
		Block,
		Text
	}

	public delegate string DebugInfoProc();

	public Mode mode = Mode.Text;

	public TextAnchor align = TextAnchor.UpperCenter;

	public float updateInterval = 0.25f;

	public Color color = new Color(0.7f, 0.7f, 0.7f);

	public int padding = 5;

	public bool showFrameTime = true;

	public bool showFullFpsInfo;

	public string numbersFormat = "0.";

	public float blockScale = 2f;

	public Color blockTextColor = Color.magenta;

	public Color blockBackgroundColor = Color.black;

	public bool showDebugInfo = true;

	public float debugInfoOffsetX = 30f;

	public float debugInfoOffsetY = 60f;

	private float lastTime;

	private int ticksTotal;

	private int framesCount;

	private float timeTotal;

	private float framesTotal;

	private float frameTime;

	private float frameTimeMax;

	private long memory;

	private long memoryMax;

	private long memorySum;

	private string text = string.Empty;

	private Texture2D texture;

	private string debugText = string.Empty;

	public DebugInfoProc debugInfo { get; set; }

	private void OnLevelWasLoaded(int level)
	{
		Reset();
	}

	public void Reset()
	{
		lastTime = Time.realtimeSinceStartup;
		ticksTotal = 0;
		framesCount = 0;
		timeTotal = 0f;
		framesTotal = 0f;
		frameTime = 0f;
		frameTimeMax = -2.1474836E+09f;
		memory = 0L;
		memoryMax = -2147483648L;
		memorySum = 0L;
	}

	private void Update()
	{
		framesCount++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - lastTime;
		if (num >= updateInterval)
		{
			ticksTotal++;
			frameTime = (realtimeSinceStartup - lastTime) / (float)framesCount;
			frameTimeMax = Mathf.Max(frameTimeMax, frameTime);
			timeTotal += num;
			framesTotal += framesCount;
			memory = NativeUtils.GetCurrentMemoryBytes();
			memory /= 1024L;
			memoryMax = (!(memory <= memoryMax) ? memory : memoryMax);
			memorySum += memory;
			lastTime = realtimeSinceStartup;
			framesCount = 0;
		}
	}

	private void UpdateTexts(bool full)
	{
		this.text = string.Empty;
		string text = ":" + numbersFormat;
		if (full)
		{
			this.text += string.Format("{0" + text + "} fps ({1" + text + "} avg, {2" + text + "} wst)", 1f / frameTime, framesTotal / timeTotal, 1f / frameTimeMax);
			if (showFrameTime)
			{
				this.text += string.Format(", {0" + text + "} ft ({1" + text + "} avg, {2" + text + "} wst)", 1000f * frameTime, 1000f * timeTotal / framesTotal, 1000f * frameTimeMax);
			}
			if (memory > 0)
			{
				this.text += string.Format(", {0" + text + "} mem ({1" + text + "} avg, {2" + text + "} wst)", (float)memory / 1024f, (float)(memorySum / ticksTotal) / 1024f, (float)memoryMax / 1024f);
			}
		}
		else
		{
			this.text += string.Format("{0" + text + "}", 1f / frameTime);
			if (showFrameTime)
			{
				this.text += string.Format(", {0" + text + "}", 1000f * frameTime);
			}
			if (memory > 0)
			{
				this.text += string.Format(", {0" + text + "}", (float)memory / 1024f);
			}
		}
		debugText = string.Empty;
		if (showDebugInfo && debugInfo != null)
		{
			Delegate[] invocationList = ((Delegate)debugInfo).GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				debugText += (invocationList[i] as DebugInfoProc)() + "\n";
			}
		}
	}

	public string GetText(bool full)
	{
		UpdateTexts(full);
		return text;
	}
}
