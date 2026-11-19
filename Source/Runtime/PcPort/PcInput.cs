// PC 桥接：用鼠标模拟触摸（做法参考 SvZ 的 PCInput：鼠标合成 Touch，替换游戏里读 Input.touchCount / GetTouch 的地方）。
// 游戏的界面和操作都走 EZ GUI 的触摸轮询（UIManager.PollTouchpad），PC 上 Input.touchCount 恒为 0，什么都点不了。
// 规则：
//   - 鼠标左键按住 = 手指 0（按下 Began，移动 Moved，不动 Stationary，松开 Ended）；
//   - InjectGesture 注入的手势 = 手指 1（自动测试用，见 PcBootDiagnostics 的 -pcAutoTap）：
//     第一帧 Began，之后按真实时间从起点线性移到终点（Moved / Stationary），到时长后一帧 Ended。
//     点击就是时长为 0 的手势：Began、Stationary、Ended 各一帧；
//   - 程序控制的手指 AimFinger = 手指 2、MoveFinger = 手指 3（自动测试用，见 PcBootDiagnostics 的 -pcAutoAim / -pcAutoMove）：
//     Active 变成 true 的那帧 Began，之后跟着 Position 走（Moved / Stationary），变回 false 的那帧 Ended；
//   - 真实触摸屏的触摸原样保留，排在后面。
// 每帧只合成一次，同一帧内多次读取结果一致。
using System.Collections.Generic;
using UnityEngine;

public static class PcInput
{
	private const int MouseFingerId = 0;

	private const int InjectedFingerId = 1;

	/// <summary>由程序控制的一根手指（自动测试用）：Active 为 true 时按在 Position（屏幕像素坐标）上。</summary>
	public class SteeredFinger
	{
		public readonly int FingerId;
		public bool Active;
		public Vector2 Position;
		public bool WasActive;
		public Vector2 LastPosition;

		public SteeredFinger(int fingerId)
		{
			FingerId = fingerId;
		}
	}

	/// <summary>自动瞄准用的手指（右半边视角 / 开火）。</summary>
	public static readonly SteeredFinger AimFinger = new SteeredFinger(2);

	/// <summary>自动行走用的手指（左摇杆）。</summary>
	public static readonly SteeredFinger MoveFinger = new SteeredFinger(3);

	/// <summary>一个注入的手势：从 start 移到 end，持续 duration 秒（真实时间）。</summary>
	private struct Gesture
	{
		public Vector2 start;
		public Vector2 end;
		public float duration;
		/// <summary>到达终点后继续按住的秒数。</summary>
		public float hold;
	}

	private static readonly List<Touch> _touches = new List<Touch>();

	private static int _builtFrame = -1;

	private static Vector2 _lastMousePosition;

	private static bool _mouseWasDown;

	private static readonly Queue<Gesture> _pendingGestures = new Queue<Gesture>();

	private static bool _injecting;

	private static Gesture _current;

	private static float _currentStartTime;

	/// <summary>当前手势已经输出了几帧（0 表示还没输出 Began）。</summary>
	private static int _currentFrames;

	private static Vector2 _currentPosition;

	/// <summary>替代 Input.touchCount。</summary>
	public static int touchCount
	{
		get
		{
			Build();
			return _touches.Count;
		}
	}

	/// <summary>替代 Input.GetTouch。</summary>
	public static Touch GetTouch(int index)
	{
		Build();
		return _touches[index];
	}

	/// <summary>替代 Input.touches。</summary>
	public static Touch[] touches
	{
		get
		{
			Build();
			return _touches.ToArray();
		}
	}

	/// <summary>在屏幕像素坐标 position 处注入一次点击（自动测试用）。</summary>
	public static void InjectTap(Vector2 position)
	{
		InjectGesture(position, position, 0f);
	}

	/// <summary>注入一个按住 / 拖动手势：从 start 移到 end（屏幕像素坐标），持续 duration 秒。</summary>
	public static void InjectGesture(Vector2 start, Vector2 end, float duration)
	{
		InjectGesture(start, end, duration, 0f);
	}

	/// <summary>注入一个拖动后按住的手势：duration 秒内从 start 移到 end，再在 end 按住 hold 秒（摇杆要这样操作）。</summary>
	public static void InjectGesture(Vector2 start, Vector2 end, float duration, float hold)
	{
		Gesture gesture = new Gesture();
		gesture.start = start;
		gesture.end = end;
		gesture.duration = duration;
		gesture.hold = hold;
		_pendingGestures.Enqueue(gesture);
		Debug.Log("[PcInput] gesture queued " + start + " -> " + end + " for " + duration + "s, hold " + hold + "s");
	}

	private static void Build()
	{
		if (_builtFrame == Time.frameCount)
		{
			return;
		}
		_builtFrame = Time.frameCount;
		_touches.Clear();
		AddMouseTouch();
		AddInjectedTouch();
		AddSteeredTouch(AimFinger);
		AddSteeredTouch(MoveFinger);
		for (int i = 0; i < Input.touchCount; i++)
		{
			_touches.Add(Input.GetTouch(i));
		}
	}

	private static void AddMouseTouch()
	{
		Vector2 position = Input.mousePosition;
		bool down = Input.GetMouseButton(0);
		Vector2 delta = position - _lastMousePosition;
		_lastMousePosition = position;
		bool wasDown = _mouseWasDown;
		_mouseWasDown = down;
		if (!down && !wasDown)
		{
			return;
		}
		Touch touch = new Touch();
		touch.fingerId = MouseFingerId;
		touch.position = position;
		touch.tapCount = 1;
		touch.deltaTime = Time.deltaTime;
		if (down && !wasDown)
		{
			touch.phase = TouchPhase.Began;
			touch.deltaPosition = Vector2.zero;
		}
		else if (!down)
		{
			touch.phase = TouchPhase.Ended;
			touch.deltaPosition = delta;
		}
		else if (delta.sqrMagnitude > 0f)
		{
			touch.phase = TouchPhase.Moved;
			touch.deltaPosition = delta;
		}
		else
		{
			touch.phase = TouchPhase.Stationary;
			touch.deltaPosition = Vector2.zero;
		}
		_touches.Add(touch);
	}

	/// <summary>按 finger.Active / finger.Position 合成一根手指。</summary>
	private static void AddSteeredTouch(SteeredFinger finger)
	{
		bool active = finger.Active;
		bool wasActive = finger.WasActive;
		finger.WasActive = active;
		if (!active && !wasActive)
		{
			return;
		}
		Touch touch = new Touch();
		touch.fingerId = finger.FingerId;
		touch.tapCount = 1;
		touch.deltaTime = Time.deltaTime;
		if (active && !wasActive)
		{
			touch.phase = TouchPhase.Began;
			touch.position = finger.Position;
			touch.deltaPosition = Vector2.zero;
		}
		else if (!active)
		{
			touch.phase = TouchPhase.Ended;
			touch.position = finger.LastPosition;
			touch.deltaPosition = Vector2.zero;
		}
		else
		{
			touch.position = finger.Position;
			touch.deltaPosition = finger.Position - finger.LastPosition;
			if (touch.deltaPosition.sqrMagnitude > 0f)
			{
				touch.phase = TouchPhase.Moved;
			}
			else
			{
				touch.phase = TouchPhase.Stationary;
			}
		}
		finger.LastPosition = touch.position;
		_touches.Add(touch);
	}

	private static void AddInjectedTouch()
	{
		if (!_injecting)
		{
			if (_pendingGestures.Count == 0)
			{
				return;
			}
			_current = _pendingGestures.Dequeue();
			_injecting = true;
			_currentFrames = 0;
			_currentStartTime = Time.realtimeSinceStartup;
			_currentPosition = _current.start;
		}
		Touch touch = new Touch();
		touch.fingerId = InjectedFingerId;
		touch.tapCount = 1;
		touch.deltaTime = Time.deltaTime;
		float elapsed = Time.realtimeSinceStartup - _currentStartTime;
		if (_currentFrames == 0)
		{
			touch.phase = TouchPhase.Began;
			touch.position = _current.start;
			touch.deltaPosition = Vector2.zero;
			Debug.Log("[PcInput] injected Began at " + _current.start);
		}
		else if (_currentFrames == 1 || elapsed < _current.duration + _current.hold)
		{
			// 至少保持一帧，再按时间插值位置
			float progress = 1f;
			if (_current.duration > 0f)
			{
				progress = Mathf.Clamp01(elapsed / _current.duration);
			}
			Vector2 position = Vector2.Lerp(_current.start, _current.end, progress);
			touch.position = position;
			touch.deltaPosition = position - _currentPosition;
			if (touch.deltaPosition.sqrMagnitude > 0f)
			{
				touch.phase = TouchPhase.Moved;
			}
			else
			{
				touch.phase = TouchPhase.Stationary;
			}
		}
		else
		{
			touch.phase = TouchPhase.Ended;
			touch.position = _current.end;
			touch.deltaPosition = _current.end - _currentPosition;
			_injecting = false;
			Debug.Log("[PcInput] injected Ended at " + _current.end + " after " + elapsed.ToString("F2") + "s");
		}
		_currentPosition = touch.position;
		_currentFrames++;
		_touches.Add(touch);
	}
}
