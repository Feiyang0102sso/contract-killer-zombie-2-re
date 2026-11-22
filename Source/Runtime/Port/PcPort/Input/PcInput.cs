// PC bridge: mouse as touch (based on SvZ's PCInput: synthesize Touch from the mouse, replacing reads of Input.touchCount / GetTouch).
// The game's UI and controls go through EZ GUI touch polling (UIManager.PollTouchpad); on PC Input.touchCount is always 0.
// Rules:
//   - LMB held = finger 0 (press Began, move Moved, still Stationary, release Ended); not synthesized while the cursor is locked (in gameplay);
//   - gestures from InjectGesture = finger 1 (auto tests, see DevBootDiagnostics -pcAutoTap):
//     Began on the first frame, then moves linearly from start to end in real time (Moved / Stationary), Ended after the duration.
//     A tap is a zero-length gesture: one frame each of Began, Stationary, Ended;
//   - steered fingers AimFinger = finger 2, MoveFinger = finger 3 (auto tests, see DevBootDiagnostics -pcAutoAim / -pcAutoMove):
//     Began when Active turns true, then follows Position (Moved / Stationary), Ended when Active turns false;
//   - real touchscreen touches are kept as is, after the synthesized ones.
// Built once per frame; repeated reads in the same frame return the same result.
using System.Collections.Generic;
using UnityEngine;

public static class PcInput
{
	private const int MouseFingerId = 0;

	private const int InjectedFingerId = 1;

	/// <summary>A program-controlled finger (auto tests): pressed at Position (screen pixels) while Active.</summary>
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

	/// <summary>Finger for auto aim (right-side look / fire).</summary>
	public static readonly SteeredFinger AimFinger = new SteeredFinger(2);

	/// <summary>Finger for auto move (left stick).</summary>
	public static readonly SteeredFinger MoveFinger = new SteeredFinger(3);

	/// <summary>An injected gesture: moves from start to end over duration seconds (real time).</summary>
	private struct Gesture
	{
		public Vector2 start;
		public Vector2 end;
		public float duration;
		/// <summary>Seconds to keep holding after reaching the end.</summary>
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

	/// <summary>Frames already output for the current gesture (0 = Began not output yet).</summary>
	private static int _currentFrames;

	private static Vector2 _currentPosition;

	/// <summary>Replaces Input.touchCount.</summary>
	public static int touchCount
	{
		get
		{
			Build();
			return _touches.Count;
		}
	}

	/// <summary>Replaces Input.GetTouch.</summary>
	public static Touch GetTouch(int index)
	{
		Build();
		return _touches[index];
	}

	/// <summary>Replaces Input.touches.</summary>
	public static Touch[] touches
	{
		get
		{
			Build();
			return _touches.ToArray();
		}
	}

	/// <summary>Injects a tap at screen pixel position (auto tests).</summary>
	public static void InjectTap(Vector2 position)
	{
		InjectGesture(position, position, 0f);
	}

	/// <summary>Injects a hold / drag gesture from start to end (screen pixels) over duration seconds.</summary>
	public static void InjectGesture(Vector2 start, Vector2 end, float duration)
	{
		InjectGesture(start, end, duration, 0f);
	}

	/// <summary>Injects a drag then hold: start to end in duration seconds, then hold at end for hold seconds (needed for sticks).</summary>
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
		// In gameplay the cursor is locked (see HudController.Pc.cs) and LMB is manual fire, so no fake touch;
		// if it was held at the moment of locking, an Ended frame is still emitted below
		bool down = Input.GetMouseButton(0) && Cursor.lockState != CursorLockMode.Locked;
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

	/// <summary>Synthesizes one finger from finger.Active / finger.Position.</summary>
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
			// Hold for at least one frame, then interpolate the position by time
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
