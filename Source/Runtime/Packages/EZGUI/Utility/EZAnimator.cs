using System.Collections.Generic;
using UnityEngine;
using System;

public class EZAnimator : MonoBehaviour
{
	private static EZAnimator s_Instance = null;

	protected static Dictionary<EZAnimation.ANIM_TYPE, EZLinkedList<EZAnimation>> freeAnimPool = new Dictionary<EZAnimation.ANIM_TYPE, EZLinkedList<EZAnimation>>();

	protected static EZLinkedList<EZAnimation> animations = new EZLinkedList<EZAnimation>();

	protected static bool pumpIsRunning = false;

	protected static bool pumpIsDone = true;

	protected static float _timeScale = 1f;

	protected static float startTime;

	protected static float time;

	protected static float elapsed;

	protected static EZAnimation anim;

	protected static float timePaused;

	private int i;

	public static EZAnimator instance
	{
		get
		{
			if (s_Instance == null)
			{
				GameObject gameObject = new GameObject("EZAnimator");
				s_Instance = (EZAnimator)gameObject.AddComponent(typeof(EZAnimator));
			}
			return s_Instance;
		}
	}

	public static bool Exists()
	{
		return s_Instance != null;
	}

	public void OnDestroy()
	{
		s_Instance = null;
	}

	public static float timeScale
	{
		get
		{
			return _timeScale;
		}
		set
		{
			_timeScale = Mathf.Max(0f, value);
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
		startTime = Time.realtimeSinceStartup;
	}

	private void OnLevelWasLoaded(int level)
	{
		if (animations == null)
		{
			return;
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			timePaused = Time.realtimeSinceStartup;
		}
		else
		{
			float num = Time.realtimeSinceStartup - timePaused;
			startTime += num;
		}
	}

	private void Update()
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		time = Time.realtimeSinceStartup;
		elapsed = (time - startTime) * _timeScale;
		startTime = time;
		eZLinkedListIterator.Begin(animations);
		while (!eZLinkedListIterator.Done)
		{
			if (!eZLinkedListIterator.Current.Step(elapsed))
			{
				anim = eZLinkedListIterator.Current;
				animations.Remove(anim);
				ReturnAnimToPool(anim);
			}
			else
			{
				eZLinkedListIterator.NextNoRemove();
			}
		}
	}

	public void StartAnimationPump()
	{
	}

	public static void StopAnimationPump()
	{
		pumpIsRunning = false;
	}

	protected EZAnimation CreateNewAnimation(EZAnimation.ANIM_TYPE type)
	{
		switch (type)
		{
		case EZAnimation.ANIM_TYPE.AnimClip:
			return new RunAnimClip();
		case EZAnimation.ANIM_TYPE.Crash:
			return new Crash();
		case EZAnimation.ANIM_TYPE.CrashRotation:
			return new CrashRotation();
		case EZAnimation.ANIM_TYPE.FadeAudio:
			return new FadeAudio();
		case EZAnimation.ANIM_TYPE.FadeSprite:
			return new FadeSprite();
		case EZAnimation.ANIM_TYPE.FadeMaterial:
			return new FadeMaterial();
		case EZAnimation.ANIM_TYPE.FadeText:
			return new FadeText();
		case EZAnimation.ANIM_TYPE.PunchPosition:
			return new PunchPosition();
		case EZAnimation.ANIM_TYPE.PunchRotation:
			return new PunchRotation();
		case EZAnimation.ANIM_TYPE.PunchScale:
			return new PunchScale();
		case EZAnimation.ANIM_TYPE.Rotate:
			return new AnimateRotation();
		case EZAnimation.ANIM_TYPE.Scale:
			return new AnimateScale();
		case EZAnimation.ANIM_TYPE.Shake:
			return new Shake();
		case EZAnimation.ANIM_TYPE.ShakeRotation:
			return new ShakeRotation();
		case EZAnimation.ANIM_TYPE.SmoothCrash:
			return new SmoothCrash();
		case EZAnimation.ANIM_TYPE.Translate:
			return new AnimatePosition();
		case EZAnimation.ANIM_TYPE.TranslateScreen:
			return new AnimateScreenPosition();
		case EZAnimation.ANIM_TYPE.TuneAudio:
			return new TuneAudio();
		case EZAnimation.ANIM_TYPE.FadeSpriteAlpha:
			return new FadeSpriteAlpha();
		case EZAnimation.ANIM_TYPE.FadeTextAlpha:
			return new FadeTextAlpha();
		case EZAnimation.ANIM_TYPE.RotateEuler:
			return new AnimateRotationEuler();
		default:
			return null;
		}
	}

	public EZAnimation GetAnimation(EZAnimation.ANIM_TYPE type)
	{
		EZLinkedList<EZAnimation> value;
		if (freeAnimPool.TryGetValue(type, out value) && !value.Empty)
		{
			EZAnimation head = value.Head;
			value.Remove(head);
			return head;
		}
		return CreateNewAnimation(type);
	}

	protected static void ReturnAnimToPool(EZAnimation anim)
	{
		anim.Clear();
		EZLinkedList<EZAnimation> value;
		if (!freeAnimPool.TryGetValue(anim.type, out value))
		{
			value = new EZLinkedList<EZAnimation>();
			freeAnimPool.Add(anim.type, value);
		}
		value.Add(anim);
		anim.ResetDefaults();
	}

	public void AddAnimation(EZAnimation a)
	{
		if (!a.running)
		{
			animations.Add(a);
			a.running = true;
		}
		StartAnimationPump();
	}

	public void AddTransition(EZTransition t)
	{
		if (t.animationTypes == null)
		{
			return;
		}
		EZAnimation eZAnimation;
		EZLinkedListNode<EZAnimation> eZLinkedListNode;
		EZLinkedList<EZLinkedListNode<GameObject>> subSubjects;
		EZAnimation.ANIM_TYPE aNIM_TYPE;
		for (int i = 0; i < t.animationTypes.Length; i++)
		{
			aNIM_TYPE = t.animationTypes[i];
			if (aNIM_TYPE == EZAnimation.ANIM_TYPE.FadeSprite || aNIM_TYPE == EZAnimation.ANIM_TYPE.FadeText || aNIM_TYPE == EZAnimation.ANIM_TYPE.FadeMaterial || aNIM_TYPE == EZAnimation.ANIM_TYPE.FadeTextAlpha || aNIM_TYPE == EZAnimation.ANIM_TYPE.FadeSpriteAlpha)
			{
				subSubjects = t.SubSubjects;
				if (subSubjects != null && subSubjects.Rewind())
				{
					do
					{
						eZAnimation = GetAnimation(aNIM_TYPE);
						t.animParams[i].transition = t;
						if (!eZAnimation.Start(subSubjects.Current.val, t.animParams[i]))
						{
							ReturnAnimToPool(eZAnimation);
						}
						else if (eZAnimation.running)
						{
							eZLinkedListNode = t.AddRunningAnim();
							eZLinkedListNode.val = eZAnimation;
							eZAnimation.Data = eZLinkedListNode;
						}
					}
					while (subSubjects.MoveNext());
				}
			}
			if (t.MainSubject == null)
			{
				continue;
			}
			eZAnimation = GetAnimation(aNIM_TYPE);
			t.animParams[i].transition = t;
			if (!eZAnimation.Start(t.MainSubject, t.animParams[i]))
			{
				ReturnAnimToPool(eZAnimation);
			}
			else if (eZAnimation.running)
			{
				eZLinkedListNode = t.AddRunningAnim();
				eZLinkedListNode.val = eZAnimation;
				eZAnimation.Data = eZLinkedListNode;
			}
		}
	}

	public void Cancel(object obj)
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		while (!eZLinkedListIterator.Done)
		{
			if (eZLinkedListIterator.Current.GetSubject() == obj)
			{
				EZAnimation current = eZLinkedListIterator.Current;
				if (current.running)
				{
					current._cancel();
					animations.Remove(current);
					ReturnAnimToPool(current);
					continue;
				}
			}
			eZLinkedListIterator.Next();
		}
		eZLinkedListIterator.End();
		if (animations.Empty)
		{
			StopAnimationPump();
		}
	}

	public void StopAnimation(EZAnimation a)
	{
		StopAnimation(a, false);
	}

	public void StopAnimation(EZAnimation a, bool end)
	{
		if (!(a.running))
		{
			return;
		}
		if (end)
		{
			a._end();
		}
		else
		{
			a._stop();
		}
		animations.Remove(a);
		ReturnAnimToPool(a);
		if (animations.Empty)
		{
			StopAnimationPump();
		}
	}

	public void Stop(object obj)
	{
		Stop(obj, false);
	}

	public void Stop(object obj, bool end)
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		while (!eZLinkedListIterator.Done)
		{
			if (eZLinkedListIterator.Current.GetSubject() == obj)
			{
				EZAnimation current = eZLinkedListIterator.Current;
				if (current.running)
				{
					if (end)
					{
						current._end();
					}
					else
					{
						current._stop();
					}
					animations.Remove(current);
					ReturnAnimToPool(current);
					continue;
				}
			}
			eZLinkedListIterator.Next();
		}
		eZLinkedListIterator.End();
		if (animations.Empty)
		{
			StopAnimationPump();
		}
	}

	public void Stop(object obj, EZAnimation.ANIM_TYPE type, bool end)
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		while (!eZLinkedListIterator.Done)
		{
			if (eZLinkedListIterator.Current.GetSubject() == obj && eZLinkedListIterator.Current.type == type)
			{
				EZAnimation current = eZLinkedListIterator.Current;
				if (current.running)
				{
					if (end)
					{
						current._end();
					}
					else
					{
						current._stop();
					}
					animations.Remove(current);
					ReturnAnimToPool(current);
					continue;
				}
			}
			eZLinkedListIterator.Next();
		}
		eZLinkedListIterator.End();
		if (animations.Empty)
		{
			StopAnimationPump();
		}
	}

	public void End(object obj)
	{
		Stop(obj, true);
	}

	public void EndAll()
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		while (!eZLinkedListIterator.Done)
		{
			eZLinkedListIterator.Current.End();
		}
		eZLinkedListIterator.End();
	}

	public void StopAll()
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		while (!eZLinkedListIterator.Done)
		{
			eZLinkedListIterator.Current.Stop();
		}
		eZLinkedListIterator.End();
	}

	public void PauseAll()
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		while (!eZLinkedListIterator.Done)
		{
			eZLinkedListIterator.Current.Paused = true;
			eZLinkedListIterator.Next();
		}
		eZLinkedListIterator.End();
	}

	public void UnpauseAll()
	{
		EZLinkedListIterator<EZAnimation> eZLinkedListIterator = animations.Begin();
		while (!eZLinkedListIterator.Done)
		{
			eZLinkedListIterator.Current.Paused = false;
			eZLinkedListIterator.Next();
		}
		eZLinkedListIterator.End();
	}

	public static int GetNumAnimations()
	{
		return animations.Count;
	}
}

public abstract class EZAnimation : IEZLinkedListItem<EZAnimation>
{
	public enum ANIM_TYPE
	{
		AnimClip,
		FadeSprite,
		FadeMaterial,
		FadeText,
		Translate,
		PunchPosition,
		Crash,
		SmoothCrash,
		Shake,
		Scale,
		PunchScale,
		Rotate,
		PunchRotation,
		ShakeRotation,
		CrashRotation,
		FadeAudio,
		TuneAudio,
		TranslateScreen,
		FadeSpriteAlpha,
		FadeTextAlpha,
		RotateEuler
	}

	public enum ANIM_MODE
	{
		By,
		To,
		FromTo
	}

	public enum EASING_TYPE
	{
		Default = -1,
		Linear,
		BackIn,
		BackOut,
		BackInOut,
		BackOutIn,
		BounceIn,
		BounceOut,
		BounceInOut,
		BounceOutIn,
		CircularIn,
		CircularOut,
		CircularInOut,
		CircularOutIn,
		CubicIn,
		CubicOut,
		CubicInOut,
		CubicOutIn,
		ElasticIn,
		ElasticOut,
		ElasticInOut,
		ElasticOutIn,
		ExponentialIn,
		ExponentialOut,
		ExponentialInOut,
		ExponentialOutIn,
		QuadraticIn,
		QuadraticOut,
		QuadraticInOut,
		QuadraticOutIn,
		QuarticIn,
		QuarticOut,
		QuarticInOut,
		QuarticOutIn,
		QuinticIn,
		QuinticOut,
		QuinticInOut,
		QuinticOutIn,
		SinusoidalIn,
		SinusoidalOut,
		SinusoidalInOut,
		SinusoidalOutIn,
		Spring
	}

	public delegate void CompletionDelegate(EZAnimation anim);

	public delegate float Interpolator(float time, float start, float delta, float duration);

	public ANIM_TYPE type;

	public bool pingPong = true;

	public bool repeatDelay;

	public bool restartOnRepeat;

	public bool running;

	protected bool m_paused;

	protected object data;

	protected ANIM_MODE m_mode;

	protected float direction = 1f;

	protected float timeElapsed;

	protected float wait;

	protected float m_wait;

	protected float duration;

	protected float interval;

	protected CompletionDelegate completedDelegate;

	protected CompletionDelegate startDelegate;

	protected Interpolator interpolator;

	protected EZAnimation m_prev;

	protected EZAnimation m_next;

	public object Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public float Duration
	{
		get
		{
			return duration;
		}
	}

	public float Wait
	{
		get
		{
			return wait;
		}
	}

	public bool Paused
	{
		get
		{
			return m_paused;
		}
		set
		{
			m_paused = running && value;
		}
	}

	public ANIM_MODE Mode
	{
		get
		{
			return m_mode;
		}
	}

	public CompletionDelegate CompletedDelegate
	{
		get
		{
			return completedDelegate;
		}
		set
		{
			completedDelegate = value;
		}
	}

	public CompletionDelegate StartDelegate
	{
		get
		{
			return startDelegate;
		}
		set
		{
			startDelegate = value;
		}
	}

	public void Clear()
	{
		completedDelegate = null;
		startDelegate = null;
		data = null;
	}

	public abstract bool Start(GameObject sub, AnimParams parms);

	protected abstract void DoAnim();

	public virtual bool Step(float timeDelta)
	{
		if (m_paused)
		{
			return true;
		}
		if (wait > 0f)
		{
			wait -= timeDelta;
			if (wait < 0f)
			{
				if (startDelegate != null)
				{
					startDelegate(this);
				}
				timeDelta -= timeDelta + wait;
				WaitDone();
			}
			else
			{
				return true;
			}
		}
		timeElapsed += timeDelta * direction;
		if (timeElapsed >= interval || timeElapsed < 0f)
		{
			if (duration < 0f)
			{
				if (pingPong)
				{
					if (timeElapsed >= interval)
					{
						direction = -1f;
						timeElapsed = interval - (timeElapsed - interval);
					}
					else
					{
						if (repeatDelay)
						{
							wait = m_wait - (timeElapsed - interval);
						}
						else
						{
							if (startDelegate != null)
							{
								startDelegate(this);
							}
							timeElapsed *= -1f;
						}
						direction = 1f;
					}
				}
				else
				{
					if (repeatDelay)
					{
						wait = m_wait;
					}
					else if (startDelegate != null)
					{
						startDelegate(this);
					}
					LoopReset();
					timeElapsed -= interval;
				}
			}
			else
			{
				_end();
				return false;
			}
		}
		DoAnim();
		return true;
	}

	public virtual void Stop()
	{
		EZAnimator.instance.StopAnimation(this);
	}

	public void _stop()
	{
		running = false;
		Paused = false;
		if (completedDelegate != null)
		{
			completedDelegate(this);
		}
	}

	public void End()
	{
		EZAnimator.instance.StopAnimation(this, true);
	}

	public void _cancel()
	{
		running = false;
		Clear();
	}

	public virtual void _end()
	{
		_stop();
	}

	protected virtual void LoopReset()
	{
	}

	public abstract object GetSubject();

	protected virtual void WaitDone()
	{
	}

	protected void StartCommon()
	{
		wait = m_wait;
		if (wait == 0f && startDelegate != null)
		{
			startDelegate(this);
		}
		interval = Mathf.Abs(duration);
		direction = 1f;
		timeElapsed = 0f;
		Paused = false;
	}

	public void ResetDefaults()
	{
		pingPong = true;
		restartOnRepeat = false;
		data = null;
		completedDelegate = null;
		startDelegate = null;
	}

	public static float linear(float time, float start, float delta, float duration)
	{
		return delta * time / duration + start;
	}

	public static float quadraticIn(float time, float start, float delta, float duration)
	{
		time /= duration;
		return delta * time * time + start;
	}

	public static float quadraticOut(float time, float start, float delta, float duration)
	{
		time /= duration;
		return (-delta) * time * (time - 2f) + start;
	}

	public static float quadraticInOut(float time, float start, float delta, float duration)
	{
		time /= duration / 2f;
		if (time < duration / 2f)
		{
			return delta / 2f * time * time + start;
		}
		time--;
		return (-delta) / 2f * (time * (time - 2f) - 1f) + start;
	}

	public static float quadraticOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return quadraticOut(time * 2f, start, delta / 2f, duration);
		}
		return quadraticIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static float cubicIn(float time, float start, float delta, float duration)
	{
		time /= duration;
		return delta * time * time * time + start;
	}

	public static float cubicOut(float time, float start, float delta, float duration)
	{
		time /= duration;
		time--;
		return delta * (time * time * time + 1f) + start;
	}

	public static float cubicInOut(float time, float start, float delta, float duration)
	{
		time /= duration / 2f;
		if (time < 1f)
		{
			return delta / 2f * time * time * time + start;
		}
		time -= 2f;
		return delta / 2f * (time * time * time + 2f) + start;
	}

	public static float cubicOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return cubicOut(time * 2f, start, delta / 2f, duration);
		}
		return cubicIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static float quarticIn(float time, float start, float delta, float duration)
	{
		time /= duration;
		return delta * time * time * time * time + start;
	}

	public static float quarticOut(float time, float start, float delta, float duration)
	{
		time /= duration;
		time--;
		return (-delta) * (time * time * time * time - 1f) + start;
	}

	public static float quarticInOut(float time, float start, float delta, float duration)
	{
		time /= duration / 2f;
		if (time < 1f)
		{
			return delta / 2f * time * time * time * time + start;
		}
		time -= 2f;
		return (-delta) / 2f * (time * time * time * time - 2f) + start;
	}

	public static float quarticOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return quarticOut(time * 2f, start, delta / 2f, duration);
		}
		return quarticIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static float quinticIn(float time, float start, float delta, float duration)
	{
		time /= duration;
		return delta * time * time * time * time * time + start;
	}

	public static float quinticOut(float time, float start, float delta, float duration)
	{
		time /= duration;
		time--;
		return delta * (time * time * time * time * time + 1f) + start;
	}

	public static float quinticInOut(float time, float start, float delta, float duration)
	{
		time /= duration / 2f;
		if (time < 1f)
		{
			return delta / 2f * time * time * time * time * time + start;
		}
		time -= 2f;
		return delta / 2f * (time * time * time * time * time + 2f) + start;
	}

	public static float quinticOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return quinticOut(time * 2f, start, delta / 2f, duration);
		}
		return quinticIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static float sinusIn(float time, float start, float delta, float duration)
	{
		return (-delta) * Mathf.Cos(time / duration * ((float)Math.PI / 2f)) + delta + start;
	}

	public static float sinusOut(float time, float start, float delta, float duration)
	{
		return delta * Mathf.Sin(time / duration * ((float)Math.PI / 2f)) + start;
	}

	public static float sinusInOut(float time, float start, float delta, float duration)
	{
		return (-delta) / 2f * (Mathf.Cos((float)Math.PI * time / duration) - 1f) + start;
	}

	public static float sinusOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return sinusOut(time * 2f, start, delta / 2f, duration);
		}
		return sinusIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static float expIn(float time, float start, float delta, float duration)
	{
		return delta * Mathf.Pow(2f, 10f * (time / duration - 1f)) + start;
	}

	public static float expOut(float time, float start, float delta, float duration)
	{
		return delta * (-Mathf.Pow(2f, -10f * time / duration) + 1f) + start;
	}

	public static float expInOut(float time, float start, float delta, float duration)
	{
		time /= duration / 2f;
		if (time < 1f)
		{
			return delta / 2f * Mathf.Pow(2f, 10f * (time - 1f)) + start;
		}
		time--;
		return delta / 2f * (-Mathf.Pow(2f, -10f * time) + 2f) + start;
	}

	public static float expOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return expOut(time * 2f, start, delta / 2f, duration);
		}
		return expIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static float circIn(float time, float start, float delta, float duration)
	{
		time /= duration;
		return (-delta) * (Mathf.Sqrt(1f - time * time) - 1f) + start;
	}

	public static float circOut(float time, float start, float delta, float duration)
	{
		time /= duration;
		time--;
		return delta * Mathf.Sqrt(1f - time * time) + start;
	}

	public static float circInOut(float time, float start, float delta, float duration)
	{
		time /= duration / 2f;
		if (time < 1f)
		{
			return (-delta) / 2f * (Mathf.Sqrt(1f - time * time) - 1f) + start;
		}
		time -= 2f;
		return delta / 2f * (Mathf.Sqrt(1f - time * time) + 1f) + start;
	}

	public static float circOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return circOut(time * 2f, start, delta / 2f, duration);
		}
		return circIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static float punch(float amplitude, float value)
	{
		float num = 9f;
		if (value == 0f)
		{
			return 0f;
		}
		if (value == 1f)
		{
			return 0f;
		}
		float num2 = 0.3f;
		num = num2 / ((float)Math.PI * 2f) * Mathf.Asin(0f);
		return amplitude * Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * 1f - num) * ((float)Math.PI * 2f) / num2);
	}

	public static float spring(float time, float start, float delta, float duration)
	{
		float value = time / duration;
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * (float)Math.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + delta * value;
	}

	public static float elasticIn(float time, float start, float delta, float duration)
	{
		return elasticIn(time, start, delta, duration, 0f, duration * 0.3f);
	}

	public static float elasticIn(float time, float start, float delta, float duration, float amplitude, float period)
	{
		if (time == 0f)
		{
			return start;
		}
		if (delta == 0f)
		{
			return start;
		}
		if ((time /= duration) == 1f)
		{
			return start + delta;
		}
		float num;
		if (amplitude < Mathf.Abs(delta))
		{
			amplitude = delta;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * Mathf.Asin(delta / amplitude);
		}
		return -(amplitude * Mathf.Pow(2f, 10f * --time) * Mathf.Sin((time * duration - num) * ((float)Math.PI * 2f) / period)) + start;
	}

	public static float elasticOut(float time, float start, float delta, float duration)
	{
		return elasticOut(time, start, delta, duration, 0f, duration * 0.3f);
	}

	public static float elasticOut(float time, float start, float delta, float duration, float amplitude, float period)
	{
		if (time == 0f)
		{
			return start;
		}
		if (delta == 0f)
		{
			return start;
		}
		if ((time /= duration) == 1f)
		{
			return start + delta;
		}
		float num;
		if (amplitude < Mathf.Abs(delta))
		{
			amplitude = delta;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * Mathf.Asin(delta / amplitude);
		}
		return amplitude * Mathf.Pow(2f, -10f * time) * Mathf.Sin((time * duration - num) * ((float)Math.PI * 2f) / period) + delta + start;
	}

	public static float elasticInOut(float time, float start, float delta, float duration)
	{
		return elasticInOut(time, start, delta, duration, 0f, duration * 0.3f * 1.5f);
	}

	public static float elasticInOut(float time, float start, float delta, float duration, float amplitude, float period)
	{
		if (time == 0f)
		{
			return start;
		}
		if (delta == 0f)
		{
			return start;
		}
		if ((time /= duration / 2f) == 2f)
		{
			return start + delta;
		}
		float num;
		if (amplitude < Mathf.Abs(delta))
		{
			amplitude = delta;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * Mathf.Asin(delta / amplitude);
		}
		if (time < 1f)
		{
			return -0.5f * (amplitude * Mathf.Pow(2f, 10f * --time) * Mathf.Sin((time * duration - num) * ((float)Math.PI * 2f) / period)) + start;
		}
		return amplitude * Mathf.Pow(2f, -10f * --time) * Mathf.Sin((time * duration - num) * ((float)Math.PI * 2f) / period) * 0.5f + delta + start;
	}

	public static float elasticOutIn(float time, float start, float delta, float duration)
	{
		return elasticOutIn(time, start, delta, duration, 0f, duration * 0.3f);
	}

	public static float elasticOutIn(float time, float start, float delta, float duration, float amplitude, float period)
	{
		if (time < duration / 2f)
		{
			return elasticOut(time * 2f, start, delta / 2f, duration, amplitude, period);
		}
		return elasticIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration, amplitude, period);
	}

	public static float backIn(float time, float start, float delta, float duration)
	{
		return backIn(time, start, delta, duration, 1.70158f);
	}

	public static float backIn(float time, float start, float delta, float duration, float overshootAmt)
	{
		return delta * (time /= duration) * time * ((overshootAmt + 1f) * time - overshootAmt) + start;
	}

	public static float backOut(float time, float start, float delta, float duration)
	{
		return backOut(time, start, delta, duration, 1.70158f);
	}

	public static float backOut(float time, float start, float delta, float duration, float overshootAmt)
	{
		return delta * ((time = time / duration - 1f) * time * ((overshootAmt + 1f) * time + overshootAmt) + 1f) + start;
	}

	public static float backInOut(float time, float start, float delta, float duration)
	{
		return backInOut(time, start, delta, duration, 1.70158f);
	}

	public static float backInOut(float time, float start, float delta, float duration, float overshootAmt)
	{
		if ((time /= duration / 2f) < 1f)
		{
			return delta / 2f * (time * time * (((overshootAmt *= 1.525f) + 1f) * time - overshootAmt)) + start;
		}
		return delta / 2f * ((time -= 2f) * time * (((overshootAmt *= 1.525f) + 1f) * time + overshootAmt) + 2f) + start;
	}

	public static float backOutIn(float time, float start, float delta, float duration)
	{
		return backOutIn(time, start, delta, duration, 1.70158f);
	}

	public static float backOutIn(float time, float start, float delta, float duration, float overshootAmt)
	{
		if (time < duration / 2f)
		{
			return backOut(time * 2f, start, delta / 2f, duration, overshootAmt);
		}
		return backIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration, overshootAmt);
	}

	public static float bounceIn(float time, float start, float delta, float duration)
	{
		return delta - bounceOut(duration - time, 0f, delta, duration) + start;
	}

	public static float bounceOut(float time, float start, float delta, float duration)
	{
		if ((time /= duration) < 372f / 1023f)
		{
			return delta * (7.5625f * time * time) + start;
		}
		if (time < 744f / 1023f)
		{
			return delta * (7.5625f * (time -= 558f / 1023f) * time + 0.75f) + start;
		}
		if (time < 930f / 1023f)
		{
			return delta * (7.5625f * (time -= 837f / 1023f) * time + 0.9375f) + start;
		}
		return delta * (7.5625f * (time -= 21f / 22f) * time + 63f / 64f) + start;
	}

	public static float bounceInOut(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return bounceIn(time * 2f, 0f, delta, duration) * 0.5f + start;
		}
		return bounceOut(time * 2f - duration, 0f, delta, duration) * 0.5f + delta * 0.5f + start;
	}

	public static float bounceOutIn(float time, float start, float delta, float duration)
	{
		if (time < duration / 2f)
		{
			return bounceOut(time * 2f, start, delta / 2f, duration);
		}
		return bounceIn(time * 2f - duration, start + delta / 2f, delta / 2f, duration);
	}

	public static Interpolator GetInterpolator(EASING_TYPE type)
	{
		switch (type)
		{
		case EASING_TYPE.BackIn:
			return backIn;
		case EASING_TYPE.BackInOut:
			return backInOut;
		case EASING_TYPE.BackOut:
			return backOut;
		case EASING_TYPE.BackOutIn:
			return backOutIn;
		case EASING_TYPE.BounceIn:
			return bounceIn;
		case EASING_TYPE.BounceInOut:
			return bounceInOut;
		case EASING_TYPE.BounceOut:
			return bounceOut;
		case EASING_TYPE.BounceOutIn:
			return bounceOutIn;
		case EASING_TYPE.CircularIn:
			return circIn;
		case EASING_TYPE.CircularInOut:
			return circInOut;
		case EASING_TYPE.CircularOut:
			return circOut;
		case EASING_TYPE.CircularOutIn:
			return circOutIn;
		case EASING_TYPE.CubicIn:
			return cubicIn;
		case EASING_TYPE.CubicInOut:
			return cubicInOut;
		case EASING_TYPE.CubicOut:
			return cubicOut;
		case EASING_TYPE.CubicOutIn:
			return cubicOutIn;
		case EASING_TYPE.ElasticIn:
			return elasticIn;
		case EASING_TYPE.ElasticInOut:
			return elasticInOut;
		case EASING_TYPE.ElasticOut:
			return elasticOut;
		case EASING_TYPE.ElasticOutIn:
			return elasticOutIn;
		case EASING_TYPE.ExponentialIn:
			return expIn;
		case EASING_TYPE.ExponentialInOut:
			return expInOut;
		case EASING_TYPE.ExponentialOut:
			return expOut;
		case EASING_TYPE.ExponentialOutIn:
			return expOutIn;
		case EASING_TYPE.Linear:
			return linear;
		case EASING_TYPE.QuadraticIn:
			return quadraticIn;
		case EASING_TYPE.QuadraticInOut:
			return quadraticInOut;
		case EASING_TYPE.QuadraticOut:
			return quadraticOut;
		case EASING_TYPE.QuadraticOutIn:
			return quadraticOutIn;
		case EASING_TYPE.QuarticIn:
			return quarticIn;
		case EASING_TYPE.QuarticInOut:
			return quarticInOut;
		case EASING_TYPE.QuarticOut:
			return quarticOut;
		case EASING_TYPE.QuarticOutIn:
			return quarticOutIn;
		case EASING_TYPE.QuinticIn:
			return quinticIn;
		case EASING_TYPE.QuinticInOut:
			return quinticInOut;
		case EASING_TYPE.QuinticOut:
			return quinticOut;
		case EASING_TYPE.QuinticOutIn:
			return quinticOutIn;
		case EASING_TYPE.SinusoidalIn:
			return sinusIn;
		case EASING_TYPE.SinusoidalInOut:
			return sinusInOut;
		case EASING_TYPE.SinusoidalOut:
			return sinusOut;
		case EASING_TYPE.SinusoidalOutIn:
			return sinusOutIn;
		case EASING_TYPE.Spring:
			return spring;
		default:
			return linear;
		}
	}

	public EZAnimation prev
	{
		get
		{
			return m_prev;
		}
		set
		{
			m_prev = value;
		}
	}

	public EZAnimation next
	{
		get
		{
			return m_next;
		}
		set
		{
			m_next = value;
		}
	}
}

public class FadeSprite : EZAnimation
{
	protected Color start;

	protected Color delta;

	protected Color end;

	protected SpriteRoot sprite;

	protected Color temp;

	public FadeSprite()
	{
		type = ANIM_TYPE.FadeSprite;
	}

	public override object GetSubject()
	{
		return sprite;
	}

	public override void _end()
	{
		if (sprite != null)
		{
			sprite.SetColor(end);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && sprite != null)
		{
			start = sprite.color;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (sprite == null)
		{
			_stop();
			return;
		}
		temp.r = interpolator(timeElapsed, start.r, delta.r, interval);
		temp.g = interpolator(timeElapsed, start.g, delta.g, interval);
		temp.b = interpolator(timeElapsed, start.b, delta.b, interval);
		temp.a = interpolator(timeElapsed, start.a, delta.a, interval);
		sprite.SetColor(temp);
	}

	public static FadeSprite Do(SpriteRoot sprt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeSprite fadeSprite = (FadeSprite)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeSprite);
		fadeSprite.Start(sprt, mode, begin, dest, interp, dur, delay, startDel, del);
		return fadeSprite;
	}

	public static FadeSprite Do(SpriteRoot sprt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeSprite fadeSprite = (FadeSprite)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeSprite);
		fadeSprite.Start(sprt, mode, dest, interp, dur, delay, startDel, del);
		return fadeSprite;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		sprite = (SpriteRoot)sub.GetComponent(typeof(SpriteRoot));
		if (sprite == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sprite, parms.mode, parms.color, parms.color2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sprite, parms.mode, sprite.color, parms.color, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(SpriteRoot sprt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(sprt, mode, sprt.color, dest, interp, dur, delay, startDel, del);
	}

	public void Start(SpriteRoot sprt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		sprite = sprt;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = new Color(dest.r - start.r, dest.g - start.g, dest.b - start.b, dest.a - start.a);
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			sprite.SetColor(start);
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (sprite == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = sprite.color;
			end = start + delta;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class FadeSpriteAlpha : EZAnimation
{
	protected Color start;

	protected Color delta;

	protected Color end;

	protected SpriteRoot sprite;

	protected Color temp;

	public FadeSpriteAlpha()
	{
		type = ANIM_TYPE.FadeSpriteAlpha;
	}

	public override object GetSubject()
	{
		return sprite;
	}

	public override void _end()
	{
		if (sprite != null)
		{
			sprite.SetColor(end);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && sprite != null)
		{
			start = sprite.color;
			end = start + delta;
		}
		temp = start;
	}

	protected override void DoAnim()
	{
		if (sprite == null)
		{
			_stop();
			return;
		}
		temp.a = interpolator(timeElapsed, start.a, delta.a, interval);
		sprite.SetColor(temp);
	}

	public static FadeSpriteAlpha Do(SpriteRoot sprt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeSpriteAlpha fadeSpriteAlpha = (FadeSpriteAlpha)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeSpriteAlpha);
		fadeSpriteAlpha.Start(sprt, mode, begin, dest, interp, dur, delay, startDel, del);
		return fadeSpriteAlpha;
	}

	public static FadeSpriteAlpha Do(SpriteRoot sprt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeSpriteAlpha fadeSpriteAlpha = (FadeSpriteAlpha)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeSpriteAlpha);
		fadeSpriteAlpha.Start(sprt, mode, dest, interp, dur, delay, startDel, del);
		return fadeSpriteAlpha;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		sprite = (SpriteRoot)sub.GetComponent(typeof(SpriteRoot));
		if (sprite == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sprite, parms.mode, parms.color, parms.color2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sprite, parms.mode, sprite.color, parms.color, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(SpriteRoot sprt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(sprt, mode, sprt.color, dest, interp, dur, delay, startDel, del);
	}

	public void Start(SpriteRoot sprt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		sprite = sprt;
		start = sprite.Color;
		start.a = begin.a;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = new Color(0f, 0f, 0f, dest.a);
		}
		else
		{
			delta = new Color(0f, 0f, 0f, dest.a - start.a);
		}
		end = start + delta;
		temp = start;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			sprite.SetColor(start);
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (sprite == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = sprite.color;
			end = start + delta;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class FadeMaterial : EZAnimation
{
	protected Color start;

	protected Color delta;

	protected Color end;

	protected Material mat;

	protected Color temp;

	public FadeMaterial()
	{
		type = ANIM_TYPE.FadeMaterial;
	}

	public override object GetSubject()
	{
		return mat;
	}

	public override void _end()
	{
		if (mat != null)
		{
			mat.color = end;
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && mat != null)
		{
			start = mat.color;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (mat == null)
		{
			_stop();
			return;
		}
		temp.r = interpolator(timeElapsed, start.r, delta.r, interval);
		temp.g = interpolator(timeElapsed, start.g, delta.g, interval);
		temp.b = interpolator(timeElapsed, start.b, delta.b, interval);
		temp.a = interpolator(timeElapsed, start.a, delta.a, interval);
		mat.color = temp;
	}

	public static FadeMaterial Do(Material material, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeMaterial fadeMaterial = (FadeMaterial)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeMaterial);
		fadeMaterial.Start(material, mode, begin, dest, interp, dur, delay, startDel, del);
		return fadeMaterial;
	}

	public static FadeMaterial Do(Material material, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeMaterial fadeMaterial = (FadeMaterial)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeMaterial);
		fadeMaterial.Start(material, mode, dest, interp, dur, delay, startDel, del);
		return fadeMaterial;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		if (sub.GetComponent<Renderer>() == null)
		{
			return false;
		}
		if (sub.GetComponent<Renderer>().material == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sub.GetComponent<Renderer>().material, parms.mode, parms.color, parms.color2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sub.GetComponent<Renderer>().material, parms.mode, sub.GetComponent<Renderer>().material.color, parms.color, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(Material material, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(material, mode, material.color, dest, interp, dur, delay, startDel, del);
	}

	public void Start(Material material, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		mat = material;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = new Color(dest.r - start.r, dest.g - start.g, dest.b - start.b, dest.a - start.a);
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			mat.color = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (mat == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = mat.color;
			end = start + delta;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class FadeText : EZAnimation
{
	protected Color start;

	protected Color delta;

	protected Color end;

	protected SpriteText text;

	protected Color temp;

	public FadeText()
	{
		type = ANIM_TYPE.FadeText;
	}

	public override object GetSubject()
	{
		return text;
	}

	public override void _end()
	{
		if (text != null)
		{
			text.SetColor(end);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && text != null)
		{
			start = text.color;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (text == null)
		{
			_stop();
			return;
		}
		temp.r = interpolator(timeElapsed, start.r, delta.r, interval);
		temp.g = interpolator(timeElapsed, start.g, delta.g, interval);
		temp.b = interpolator(timeElapsed, start.b, delta.b, interval);
		temp.a = interpolator(timeElapsed, start.a, delta.a, interval);
		text.SetColor(temp);
	}

	public static FadeText Do(SpriteText txt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeText fadeText = (FadeText)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeText);
		fadeText.Start(txt, mode, begin, dest, interp, dur, delay, startDel, del);
		return fadeText;
	}

	public static FadeText Do(SpriteText txt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeText fadeText = (FadeText)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeText);
		fadeText.Start(txt, mode, dest, interp, dur, delay, startDel, del);
		return fadeText;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		text = (SpriteText)sub.GetComponent(typeof(SpriteText));
		if (text == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(text, parms.mode, parms.color, parms.color2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(text, parms.mode, text.color, parms.color, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(SpriteText txt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(txt, mode, txt.color, dest, interp, dur, delay, startDel, del);
	}

	public void Start(SpriteText txt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		text = txt;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = new Color(dest.r - start.r, dest.g - start.g, dest.b - start.b, dest.a - start.a);
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			text.SetColor(start);
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class FadeTextAlpha : EZAnimation
{
	protected Color start;

	protected Color delta;

	protected Color end;

	protected SpriteText text;

	protected Color temp;

	public FadeTextAlpha()
	{
		type = ANIM_TYPE.FadeTextAlpha;
	}

	public override object GetSubject()
	{
		return text;
	}

	public override void _end()
	{
		if (text != null)
		{
			text.SetColor(end);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && text != null)
		{
			start = text.color;
			end = start + delta;
		}
		temp = start;
	}

	protected override void DoAnim()
	{
		if (text == null)
		{
			_stop();
			return;
		}
		temp.a = interpolator(timeElapsed, start.a, delta.a, interval);
		text.SetColor(temp);
	}

	public static FadeTextAlpha Do(SpriteText txt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeTextAlpha fadeTextAlpha = (FadeTextAlpha)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeTextAlpha);
		fadeTextAlpha.Start(txt, mode, begin, dest, interp, dur, delay, startDel, del);
		return fadeTextAlpha;
	}

	public static FadeTextAlpha Do(SpriteText txt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeTextAlpha fadeTextAlpha = (FadeTextAlpha)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeTextAlpha);
		fadeTextAlpha.Start(txt, mode, dest, interp, dur, delay, startDel, del);
		return fadeTextAlpha;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		text = (SpriteText)sub.GetComponent(typeof(SpriteText));
		if (text == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(text, parms.mode, parms.color, parms.color2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(text, parms.mode, text.color, parms.color, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(SpriteText txt, ANIM_MODE mode, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(txt, mode, txt.color, dest, interp, dur, delay, startDel, del);
	}

	public void Start(SpriteText txt, ANIM_MODE mode, Color begin, Color dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		text = txt;
		start = txt.Color;
		start.a = begin.a;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = new Color(0f, 0f, 0f, dest.a);
		}
		else
		{
			delta = new Color(0f, 0f, 0f, dest.a - start.a);
		}
		end = start + delta;
		temp = start;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			text.SetColor(start);
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class AnimateRotation : EZAnimation
{
	protected GameObject subject;

	protected Transform subTrans;

	protected Quaternion temp;

	protected Quaternion delta;

	protected Quaternion start;

	protected Quaternion end;

	public AnimateRotation()
	{
		type = ANIM_TYPE.Rotate;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localRotation = end;
			subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end.x = start.x + delta.x;
			end.y = start.y + delta.y;
			end.z = start.z + delta.z;
			end.w = start.w + delta.w;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && subTrans != null)
		{
			start = subTrans.localRotation;
			end.x = start.x + delta.x;
			end.y = start.y + delta.y;
			end.z = start.z + delta.z;
			end.w = start.w + delta.w;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		temp = Quaternion.Slerp(start, end, interpolator(timeElapsed, 0f, 1f, interval));
		subTrans.localRotation = temp;
		subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
	}

	public static AnimateRotation Do(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateRotation animateRotation = (AnimateRotation)EZAnimator.instance.GetAnimation(ANIM_TYPE.Rotate);
		animateRotation.Start(sub, mode, begin, dest, interp, dur, delay, startDel, del);
		return animateRotation;
	}

	public static AnimateRotation Do(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateRotation animateRotation = (AnimateRotation)EZAnimator.instance.GetAnimation(ANIM_TYPE.Rotate);
		animateRotation.Start(sub, mode, dest, interp, dur, delay, startDel, del);
		return animateRotation;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sub, parms.mode, parms.vec, parms.vec2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sub, parms.mode, sub.transform.localEulerAngles, parms.vec, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, mode, sub.transform.localEulerAngles, dest, interp, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = Quaternion.Euler(begin);
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			Quaternion quaternion = Quaternion.Euler(begin + dest);
			delta = new Quaternion(quaternion.x - start.x, quaternion.y - start.y, quaternion.z - start.z, quaternion.w - start.w);
		}
		else
		{
			Quaternion quaternion2 = Quaternion.Euler(dest);
			delta = new Quaternion(quaternion2.x - start.x, quaternion2.y - start.y, quaternion2.z - start.z, quaternion2.w - start.w);
		}
		end.x = start.x + delta.x;
		end.y = start.y + delta.y;
		end.z = start.z + delta.z;
		end.w = start.w + delta.w;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			subTrans.localRotation = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (subject == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = subject.transform.localRotation;
			end.x = start.x + delta.x;
			end.y = start.y + delta.y;
			end.z = start.z + delta.z;
			end.w = start.w + delta.w;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class AnimateRotationEuler : EZAnimation
{
	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 start;

	protected Vector3 delta;

	protected Vector3 end;

	protected Vector3 temp;

	public AnimateRotationEuler()
	{
		type = ANIM_TYPE.RotateEuler;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localEulerAngles = end;
			subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && subTrans != null)
		{
			start = subTrans.localEulerAngles;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		temp.x = interpolator(timeElapsed, start.x, delta.x, interval);
		temp.y = interpolator(timeElapsed, start.y, delta.y, interval);
		temp.z = interpolator(timeElapsed, start.z, delta.z, interval);
		subTrans.localEulerAngles = temp;
		subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
	}

	public static AnimateRotationEuler Do(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateRotationEuler animateRotationEuler = (AnimateRotationEuler)EZAnimator.instance.GetAnimation(ANIM_TYPE.RotateEuler);
		animateRotationEuler.Start(sub, mode, begin, dest, interp, dur, delay, startDel, del);
		return animateRotationEuler;
	}

	public static AnimateRotationEuler Do(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateRotationEuler animateRotationEuler = (AnimateRotationEuler)EZAnimator.instance.GetAnimation(ANIM_TYPE.RotateEuler);
		animateRotationEuler.Start(sub, mode, dest, interp, dur, delay, startDel, del);
		return animateRotationEuler;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sub, parms.mode, parms.vec, parms.vec2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sub, parms.mode, sub.transform.localEulerAngles, parms.vec, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, mode, sub.transform.localEulerAngles, dest, interp, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = new Vector3(dest.x - start.x, dest.y - start.y, dest.z - start.z);
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			subTrans.localEulerAngles = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (subject == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = subject.transform.localEulerAngles;
			end = start + delta;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class AnimatePosition : EZAnimation
{
	protected Vector3 start;

	protected Vector3 delta;

	protected Vector3 end;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	public AnimatePosition()
	{
		type = ANIM_TYPE.Translate;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localPosition = end;
			subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && subTrans != null)
		{
			start = subTrans.localPosition;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		temp.x = interpolator(timeElapsed, start.x, delta.x, interval);
		temp.y = interpolator(timeElapsed, start.y, delta.y, interval);
		temp.z = interpolator(timeElapsed, start.z, delta.z, interval);
		subTrans.localPosition = temp;
		subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
	}

	public static AnimatePosition Do(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimatePosition animatePosition = (AnimatePosition)EZAnimator.instance.GetAnimation(ANIM_TYPE.Translate);
		animatePosition.Start(sub, mode, begin, dest, interp, dur, delay, startDel, del);
		return animatePosition;
	}

	public static AnimatePosition Do(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimatePosition animatePosition = (AnimatePosition)EZAnimator.instance.GetAnimation(ANIM_TYPE.Translate);
		animatePosition.Start(sub, mode, dest, interp, dur, delay, startDel, del);
		return animatePosition;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sub, parms.mode, parms.vec, parms.vec2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sub, parms.mode, sub.transform.localPosition, parms.vec, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, mode, sub.transform.localPosition, dest, interp, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = dest - start;
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			subTrans.localPosition = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (subject == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = subject.transform.localPosition;
			end = start + delta;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class AnimateScreenPosition : EZAnimation
{
	protected Vector3 start;

	protected Vector3 delta;

	protected Vector3 end;

	protected GameObject subject;

	protected EZScreenPlacement subSP;

	protected Vector3 temp;

	public AnimateScreenPosition()
	{
		type = ANIM_TYPE.TranslateScreen;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subSP != null)
		{
			subSP.screenPos = end;
			subSP.SetCamera();
			subSP.PositionOnScreen();
			subSP.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && subSP != null)
		{
			start = subSP.screenPos;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (subSP == null)
		{
			_stop();
			return;
		}
		temp.x = interpolator(timeElapsed, start.x, delta.x, interval);
		temp.y = interpolator(timeElapsed, start.y, delta.y, interval);
		temp.z = interpolator(timeElapsed, start.z, delta.z, interval);
		subSP.screenPos = temp;
		subSP.PositionOnScreen();
		subSP.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
	}

	public static AnimateScreenPosition Do(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateScreenPosition animateScreenPosition = (AnimateScreenPosition)EZAnimator.instance.GetAnimation(ANIM_TYPE.TranslateScreen);
		animateScreenPosition.Start(sub, mode, begin, dest, interp, dur, delay, startDel, del);
		return animateScreenPosition;
	}

	public static AnimateScreenPosition Do(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateScreenPosition animateScreenPosition = (AnimateScreenPosition)EZAnimator.instance.GetAnimation(ANIM_TYPE.TranslateScreen);
		animateScreenPosition.Start(sub, mode, dest, interp, dur, delay, startDel, del);
		return animateScreenPosition;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		EZScreenPlacement eZScreenPlacement = (EZScreenPlacement)sub.GetComponent(typeof(EZScreenPlacement));
		if (eZScreenPlacement == null)
		{
			Debug.LogError(string.Format("{0} has no EZScreenPlacement attached - but it's required for using AnimateScreenPosition/TranslateScreen!", sub.name));
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sub, parms.mode, parms.vec, parms.vec2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sub, parms.mode, eZScreenPlacement.screenPos, parms.vec, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		EZScreenPlacement eZScreenPlacement = (EZScreenPlacement)sub.GetComponent(typeof(EZScreenPlacement));
		if (eZScreenPlacement == null)
		{
			Debug.LogError(string.Format("{0} has no EZScreenPlacement attached - but it's required for using AnimateScreenPosition/TranslateScreen!", sub.name));
			return;
		}
		Start(sub, mode, eZScreenPlacement.screenPos, dest, interp, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		EZScreenPlacement eZScreenPlacement = (EZScreenPlacement)sub.GetComponent(typeof(EZScreenPlacement));
		if (eZScreenPlacement == null)
		{
			Debug.LogError(string.Format("{0} has no EZScreenPlacement attached - but it's required for using AnimateScreenPosition/TranslateScreen!", sub.name));
			return;
		}
		subject = sub;
		subSP = eZScreenPlacement;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = dest - start;
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			subSP.screenPos = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (subject == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = subject.transform.localEulerAngles;
			end = start + delta;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class AnimateScale : EZAnimation
{
	protected Vector3 start;

	protected Vector3 delta;

	protected Vector3 end;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	public AnimateScale()
	{
		type = ANIM_TYPE.Scale;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localScale = end;
			subTrans.BroadcastMessage("OnEZScaled", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && subTrans != null)
		{
			start = subTrans.localScale;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		temp.x = interpolator(timeElapsed, start.x, delta.x, interval);
		temp.y = interpolator(timeElapsed, start.y, delta.y, interval);
		temp.z = interpolator(timeElapsed, start.z, delta.z, interval);
		subTrans.localScale = temp;
		subTrans.BroadcastMessage("OnEZScaled", SendMessageOptions.DontRequireReceiver);
	}

	public static AnimateScale Do(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateScale animateScale = (AnimateScale)EZAnimator.instance.GetAnimation(ANIM_TYPE.Scale);
		animateScale.Start(sub, mode, begin, dest, interp, dur, delay, startDel, del);
		return animateScale;
	}

	public static AnimateScale Do(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateScale animateScale = (AnimateScale)EZAnimator.instance.GetAnimation(ANIM_TYPE.Scale);
		animateScale.Start(sub, mode, dest, interp, dur, delay, startDel, del);
		return animateScale;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sub, parms.mode, parms.vec, parms.vec2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sub, parms.mode, sub.transform.localScale, parms.vec, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, mode, sub.transform.localScale, dest, interp, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = Vector3.Scale(start, dest) - start;
		}
		else
		{
			delta = dest - start;
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			subTrans.localScale = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}

	public void Start()
	{
		if (subject == null)
		{
			return;
		}
		direction = 1f;
		timeElapsed = 0f;
		wait = m_wait;
		if (m_mode == ANIM_MODE.By)
		{
			start = subject.transform.localScale;
			end = start + delta;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class PunchPosition : EZAnimation
{
	protected Vector3 start;

	protected Vector3 magnitude;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	protected float factor;

	public PunchPosition()
	{
		type = ANIM_TYPE.PunchPosition;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localPosition = start;
			subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localPosition;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = timeElapsed / interval;
		temp.x = start.x + EZAnimation.punch(magnitude.x, factor);
		temp.y = start.y + EZAnimation.punch(magnitude.y, factor);
		temp.z = start.z + EZAnimation.punch(magnitude.z, factor);
		subTrans.localPosition = temp;
		subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
	}

	public static PunchPosition Do(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		PunchPosition punchPosition = (PunchPosition)EZAnimator.instance.GetAnimation(ANIM_TYPE.PunchPosition);
		punchPosition.Start(sub, mag, dur, delay, startDel, del);
		return punchPosition;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localPosition, parms.vec, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localPosition, mag, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localPosition = start;
		if (mag.x < 0f)
		{
			mag.x = UnityEngine.Random.Range(1f, -mag.x);
		}
		if (mag.y < 0f)
		{
			mag.y = UnityEngine.Random.Range(1f, -mag.y);
		}
		if (mag.z < 0f)
		{
			mag.z = UnityEngine.Random.Range(1f, -mag.z);
		}
		magnitude = mag;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class PunchScale : EZAnimation
{
	protected Vector3 start;

	protected Vector3 magnitude;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	protected float factor;

	public PunchScale()
	{
		type = ANIM_TYPE.PunchScale;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localScale = start;
			subTrans.BroadcastMessage("OnEZScaled", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localScale;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = timeElapsed / interval;
		temp.x = start.x + EZAnimation.punch(magnitude.x, factor);
		temp.y = start.y + EZAnimation.punch(magnitude.y, factor);
		temp.z = start.z + EZAnimation.punch(magnitude.z, factor);
		subTrans.localScale = temp;
		subTrans.BroadcastMessage("OnEZScaled", SendMessageOptions.DontRequireReceiver);
	}

	public static PunchScale Do(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		PunchScale punchScale = (PunchScale)EZAnimator.instance.GetAnimation(ANIM_TYPE.PunchScale);
		punchScale.Start(sub, mag, dur, delay, startDel, del);
		return punchScale;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localScale, parms.vec, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localScale, mag, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localScale = start;
		if (mag.x < 0f)
		{
			mag.x = UnityEngine.Random.Range(1f, -mag.x);
		}
		if (mag.y < 0f)
		{
			mag.y = UnityEngine.Random.Range(1f, -mag.y);
		}
		if (mag.z < 0f)
		{
			mag.z = UnityEngine.Random.Range(1f, -mag.z);
		}
		magnitude = mag;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class PunchRotation : EZAnimation
{
	protected Vector3 start;

	protected Vector3 magnitude;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	protected float factor;

	public PunchRotation()
	{
		type = ANIM_TYPE.PunchRotation;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localEulerAngles = start;
			subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localEulerAngles;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = timeElapsed / interval;
		temp.x = start.x + EZAnimation.punch(magnitude.x, factor);
		temp.y = start.y + EZAnimation.punch(magnitude.y, factor);
		temp.z = start.z + EZAnimation.punch(magnitude.z, factor);
		subTrans.localRotation = Quaternion.Euler(temp);
		subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
	}

	public static PunchRotation Do(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		PunchRotation punchRotation = (PunchRotation)EZAnimator.instance.GetAnimation(ANIM_TYPE.PunchRotation);
		punchRotation.Start(sub, mag, dur, delay, startDel, del);
		return punchRotation;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localEulerAngles, parms.vec, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localEulerAngles, mag, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localEulerAngles = start;
		if (mag.x < 0f)
		{
			mag.x = UnityEngine.Random.Range(1f, -mag.x);
		}
		if (mag.y < 0f)
		{
			mag.y = UnityEngine.Random.Range(1f, -mag.y);
		}
		if (mag.z < 0f)
		{
			mag.z = UnityEngine.Random.Range(1f, -mag.z);
		}
		magnitude = mag;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class Crash : EZAnimation
{
	protected Vector3 start;

	protected Vector3 magnitude;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 tempMag;

	protected Vector3 temp;

	protected float factor;

	public Crash()
	{
		type = ANIM_TYPE.Crash;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localPosition = start;
			subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localPosition;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = timeElapsed / interval;
		tempMag.x = magnitude.x - factor * magnitude.x;
		tempMag.y = magnitude.y - factor * magnitude.y;
		tempMag.z = magnitude.z - factor * magnitude.z;
		temp.x = start.x + UnityEngine.Random.Range(-tempMag.x, tempMag.x);
		temp.y = start.y + UnityEngine.Random.Range(-tempMag.y, tempMag.y);
		temp.z = start.z + UnityEngine.Random.Range(-tempMag.z, tempMag.z);
		subTrans.localPosition = temp;
		subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
	}

	public static Crash Do(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Crash crash = (Crash)EZAnimator.instance.GetAnimation(ANIM_TYPE.Crash);
		crash.Start(sub, mag, dur, delay, startDel, del);
		return crash;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localPosition, parms.vec, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localPosition, mag, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localPosition = start;
		if (mag.x < 0f)
		{
			mag.x = UnityEngine.Random.Range(1f, -mag.x);
		}
		if (mag.y < 0f)
		{
			mag.y = UnityEngine.Random.Range(1f, -mag.y);
		}
		if (mag.z < 0f)
		{
			mag.z = UnityEngine.Random.Range(1f, -mag.z);
		}
		magnitude = mag;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class SmoothCrash : EZAnimation
{
	protected const float PIx2 = (float)Math.PI * 2f;

	protected Vector3 start;

	protected Vector3 magnitude;

	protected Vector3 oscillations;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	protected float factor;

	protected float invFactor;

	public SmoothCrash()
	{
		type = ANIM_TYPE.SmoothCrash;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localPosition = start;
			subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localPosition;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = timeElapsed / interval;
		invFactor = 1f - factor;
		factor *= (float)Math.PI * 2f;
		temp.x = start.x + Mathf.Sin(factor * oscillations.x) * magnitude.x * invFactor;
		temp.y = start.y + Mathf.Sin(factor * oscillations.y) * magnitude.y * invFactor;
		temp.z = start.z + Mathf.Sin(factor * oscillations.z) * magnitude.z * invFactor;
		subTrans.localPosition = temp;
		subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
	}

	public static SmoothCrash Do(GameObject sub, Vector3 mag, Vector3 oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		SmoothCrash smoothCrash = (SmoothCrash)EZAnimator.instance.GetAnimation(ANIM_TYPE.SmoothCrash);
		smoothCrash.Start(sub, mag, oscill, dur, delay, startDel, del);
		return smoothCrash;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localPosition, parms.vec, parms.vec2, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, Vector3 oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localPosition, mag, oscill, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, Vector3 oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localPosition = start;
		if (mag.x < 0f)
		{
			mag.x = UnityEngine.Random.Range(1f, -mag.x);
		}
		if (mag.y < 0f)
		{
			mag.y = UnityEngine.Random.Range(1f, -mag.y);
		}
		if (mag.z < 0f)
		{
			mag.z = UnityEngine.Random.Range(1f, -mag.z);
		}
		magnitude = mag;
		if (oscill.x < 0f)
		{
			oscill.x = UnityEngine.Random.Range(1f, -oscill.x);
		}
		if (oscill.y < 0f)
		{
			oscill.y = UnityEngine.Random.Range(1f, -oscill.y);
		}
		if (oscill.z < 0f)
		{
			oscill.z = UnityEngine.Random.Range(1f, -oscill.z);
		}
		oscillations = oscill;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class Shake : EZAnimation
{
	protected const float PIx2 = (float)Math.PI * 2f;

	protected Vector3 start;

	protected Vector3 magnitude;

	protected float oscillations;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	protected float factor;

	public Shake()
	{
		type = ANIM_TYPE.Shake;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localPosition = start;
			subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localPosition;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = Mathf.Sin(timeElapsed / interval * ((float)Math.PI * 2f) * oscillations);
		temp.x = start.x + factor * magnitude.x;
		temp.y = start.y + factor * magnitude.y;
		temp.z = start.z + factor * magnitude.z;
		subTrans.localPosition = temp;
		subTrans.BroadcastMessage("OnEZTranslated", SendMessageOptions.DontRequireReceiver);
	}

	public static Shake Do(GameObject sub, Vector3 mag, float oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Shake shake = (Shake)EZAnimator.instance.GetAnimation(ANIM_TYPE.Shake);
		shake.Start(sub, mag, oscill, dur, delay, startDel, del);
		return shake;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localPosition, parms.vec, parms.floatVal, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, float oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localPosition, mag, oscill, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, float oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localPosition = start;
		if (mag.x < 0f)
		{
			mag.x = UnityEngine.Random.Range(1f, -mag.x);
		}
		if (mag.y < 0f)
		{
			mag.y = UnityEngine.Random.Range(1f, -mag.y);
		}
		if (mag.z < 0f)
		{
			mag.z = UnityEngine.Random.Range(1f, -mag.z);
		}
		if (oscill < 0f)
		{
			oscill = UnityEngine.Random.Range(1f, -oscill);
		}
		magnitude = mag;
		oscillations = oscill;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class CrashRotation : EZAnimation
{
	protected const float PIx2 = (float)Math.PI * 2f;

	protected Vector3 start;

	protected Vector3 magnitude;

	protected Vector3 oscillations;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	protected float factor;

	protected float invFactor;

	public CrashRotation()
	{
		type = ANIM_TYPE.CrashRotation;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localEulerAngles = start;
			subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localEulerAngles;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = timeElapsed / interval;
		invFactor = 1f - factor;
		factor *= (float)Math.PI * 2f;
		temp.x = start.x + Mathf.Sin(factor * oscillations.x) * magnitude.x * invFactor;
		temp.y = start.y + Mathf.Sin(factor * oscillations.y) * magnitude.y * invFactor;
		temp.z = start.z + Mathf.Sin(factor * oscillations.z) * magnitude.z * invFactor;
		subTrans.localRotation = Quaternion.Euler(temp);
		subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
	}

	public static CrashRotation Do(GameObject sub, Vector3 mag, Vector3 oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		CrashRotation crashRotation = (CrashRotation)EZAnimator.instance.GetAnimation(ANIM_TYPE.CrashRotation);
		crashRotation.Start(sub, mag, oscill, dur, delay, startDel, del);
		return crashRotation;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localEulerAngles, parms.vec, parms.vec2, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, Vector3 oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localEulerAngles, mag, oscill, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, Vector3 oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localEulerAngles = start;
		if (mag.x < 0f)
		{
			mag.x = UnityEngine.Random.Range(1f, -mag.x);
		}
		if (mag.y < 0f)
		{
			mag.y = UnityEngine.Random.Range(1f, -mag.y);
		}
		if (mag.z < 0f)
		{
			mag.z = UnityEngine.Random.Range(1f, -mag.z);
		}
		if (oscill.x < 0f)
		{
			oscill.x = UnityEngine.Random.Range(1f, -oscill.x);
		}
		if (oscill.y < 0f)
		{
			oscill.y = UnityEngine.Random.Range(1f, -oscill.y);
		}
		if (oscill.z < 0f)
		{
			oscill.z = UnityEngine.Random.Range(1f, -oscill.z);
		}
		magnitude = mag;
		oscillations = oscill;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class ShakeRotation : EZAnimation
{
	protected const float PIx2 = (float)Math.PI * 2f;

	protected Vector3 start;

	protected Vector3 magnitude;

	protected float oscillations;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	protected float factor;

	public ShakeRotation()
	{
		type = ANIM_TYPE.ShakeRotation;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localEulerAngles = start;
			subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
		}
		base._end();
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (subTrans != null)
		{
			start = subTrans.localEulerAngles;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		factor = Mathf.Sin(timeElapsed / interval * ((float)Math.PI * 2f) * oscillations);
		temp.x = start.x + factor * magnitude.x;
		temp.y = start.y + factor * magnitude.y;
		temp.z = start.z + factor * magnitude.z;
		subTrans.localRotation = Quaternion.Euler(temp);
		subTrans.BroadcastMessage("OnEZRotated", SendMessageOptions.DontRequireReceiver);
	}

	public static ShakeRotation Do(GameObject sub, Vector3 mag, float oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		ShakeRotation shakeRotation = (ShakeRotation)EZAnimator.instance.GetAnimation(ANIM_TYPE.ShakeRotation);
		shakeRotation.Start(sub, mag, oscill, dur, delay, startDel, del);
		return shakeRotation;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, sub.transform.localEulerAngles, parms.vec, parms.floatVal, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, Vector3 mag, float oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		Start(sub, sub.transform.localEulerAngles, mag, oscill, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, Vector3 begin, Vector3 mag, float oscill, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		subTrans = subject.transform;
		start = begin;
		subTrans.localEulerAngles = start;
		magnitude = mag;
		oscillations = oscill;
		m_mode = ANIM_MODE.By;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.Stop(subject, type, true);
		EZAnimator.instance.AddAnimation(this);
	}
}

public class RunAnimClip : EZAnimation
{
	protected GameObject subject;

	protected string m_clip;

	protected bool waitForClip = true;

	protected bool playedYet;

	protected float blending;

	public RunAnimClip()
	{
		type = ANIM_TYPE.AnimClip;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override bool Step(float timeDelta)
	{
		if (wait > 0f)
		{
			wait -= timeDelta;
			if (wait < 0f)
			{
				timeDelta -= timeDelta + wait;
			}
			else
			{
				return true;
			}
		}
		if (!playedYet)
		{
			if (duration == 0f && blending == 0f)
			{
				subject.GetComponent<Animation>().Play(m_clip);
			}
			else
			{
				subject.GetComponent<Animation>().Blend(m_clip, blending, duration);
			}
			playedYet = true;
			return true;
		}
		if (subject.GetComponent<Animation>().IsPlaying(m_clip))
		{
			return true;
		}
		_end();
		return false;
	}

	protected override void DoAnim()
	{
	}

	public static RunAnimClip Do(GameObject sub, string clip, float blend, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		RunAnimClip runAnimClip = (RunAnimClip)EZAnimator.instance.GetAnimation(ANIM_TYPE.AnimClip);
		runAnimClip.Start(sub, clip, blend, dur, delay, startDel, del);
		return runAnimClip;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		if (sub.GetComponent<Animation>() == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		Start(sub, parms.strVal, parms.floatVal, parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		return true;
	}

	public void Start(GameObject sub, string clip, float blend, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (sub == null)
		{
			return;
		}
		if (sub.GetComponent<Animation>() == null)
		{
			return;
		}
		playedYet = false;
		subject = sub;
		m_clip = clip;
		blending = blend;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		EZAnimator.instance.AddAnimation(this);
	}
}

public class FadeAudio : EZAnimation
{
	protected float start;

	protected AudioSource subject;

	protected float delta;

	protected float end;

	public FadeAudio()
	{
		type = ANIM_TYPE.FadeAudio;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subject != null)
		{
			subject.volume = end;
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && subject != null)
		{
			start = subject.volume;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (subject == null)
		{
			_stop();
			return;
		}
		subject.volume = interpolator(timeElapsed, start, delta, interval);
	}

	public static FadeAudio Do(AudioSource audio, ANIM_MODE mode, float begin, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeAudio fadeAudio = (FadeAudio)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeAudio);
		fadeAudio.Start(audio, mode, begin, dest, interp, dur, delay, startDel, del);
		return fadeAudio;
	}

	public static FadeAudio Do(AudioSource audio, ANIM_MODE mode, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		FadeAudio fadeAudio = (FadeAudio)EZAnimator.instance.GetAnimation(ANIM_TYPE.FadeAudio);
		fadeAudio.Start(audio, mode, dest, interp, dur, delay, startDel, del);
		return fadeAudio;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		subject = (AudioSource)sub.GetComponent(typeof(AudioSource));
		if (subject == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(subject, parms.mode, parms.floatVal, parms.floatVal2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(subject, parms.mode, subject.volume, parms.floatVal, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(AudioSource audio, ANIM_MODE mode, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(audio, mode, audio.volume, dest, interp, dur, delay, startDel, del);
	}

	public void Start(AudioSource sub, ANIM_MODE mode, float begin, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = dest - start;
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			subject.volume = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

public class TuneAudio : EZAnimation
{
	protected float start;

	protected AudioSource subject;

	protected float delta;

	protected float end;

	public TuneAudio()
	{
		type = ANIM_TYPE.TuneAudio;
		pingPong = false;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subject != null)
		{
			subject.volume = end;
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void WaitDone()
	{
		base.WaitDone();
		if (Mode == ANIM_MODE.By && subject != null)
		{
			start = subject.pitch;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (subject == null)
		{
			_stop();
			return;
		}
		subject.pitch = interpolator(timeElapsed, start, delta, interval);
	}

	public static TuneAudio Do(AudioSource audio, ANIM_MODE mode, float begin, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		TuneAudio tuneAudio = (TuneAudio)EZAnimator.instance.GetAnimation(ANIM_TYPE.TuneAudio);
		tuneAudio.Start(audio, mode, begin, dest, interp, dur, delay, startDel, del);
		return tuneAudio;
	}

	public static TuneAudio Do(AudioSource audio, ANIM_MODE mode, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		TuneAudio tuneAudio = (TuneAudio)EZAnimator.instance.GetAnimation(ANIM_TYPE.TuneAudio);
		tuneAudio.Start(audio, mode, dest, interp, dur, delay, startDel, del);
		return tuneAudio;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		if (sub == null)
		{
			return false;
		}
		subject = (AudioSource)sub.GetComponent(typeof(AudioSource));
		if (subject == null)
		{
			return false;
		}
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(subject, parms.mode, parms.floatVal, parms.floatVal2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(subject, parms.mode, subject.pitch, parms.floatVal, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(AudioSource audio, ANIM_MODE mode, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(audio, mode, audio.pitch, dest, interp, dur, delay, startDel, del);
	}

	public void Start(AudioSource sub, ANIM_MODE mode, float begin, float dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		subject = sub;
		start = begin;
		m_mode = mode;
		if (mode == ANIM_MODE.By)
		{
			delta = dest;
		}
		else
		{
			delta = dest - start;
		}
		end = start + delta;
		interpolator = interp;
		duration = dur;
		m_wait = delay;
		completedDelegate = del;
		startDelegate = startDel;
		StartCommon();
		if (mode == ANIM_MODE.FromTo && delay == 0f)
		{
			subject.pitch = start;
		}
		EZAnimator.instance.AddAnimation(this);
	}
}

[Serializable]
public class AnimParams
{
	public EZAnimation.ANIM_MODE mode = EZAnimation.ANIM_MODE.To;

	public float delay;

	public float duration;

	public bool pingPong;

	public bool restartOnRepeat;

	public bool repeatDelay;

	public EZAnimation.EASING_TYPE easing;

	[NonSerialized]
	protected EZTransition m_transition;

	public Color color = Color.white;

	public Color color2 = Color.white;

	public Vector3 vec;

	public Vector3 vec2;

	public Vector3 axis;

	public float floatVal;

	public float floatVal2;

	public string strVal = string.Empty;

	public void Copy(AnimParams src)
	{
		mode = src.mode;
		delay = src.delay;
		duration = src.duration;
		easing = src.easing;
		color = src.color;
		vec = src.vec;
		axis = src.axis;
		floatVal = src.floatVal;
		color2 = src.color2;
		vec2 = src.vec2;
		floatVal2 = src.floatVal2;
		strVal = src.strVal;
		pingPong = src.pingPong;
		repeatDelay = src.repeatDelay;
		restartOnRepeat = src.restartOnRepeat;
	}

	public AnimParams(EZTransition trans)
	{
		m_transition = trans;
	}

	public EZTransition transition
	{
		get
		{
			return m_transition;
		}
		set
		{
			m_transition = value;
		}
	}

	public virtual void DrawGUI(EZAnimation.ANIM_TYPE type, GameObject go, IGUIHelper gui, bool inspector)
	{
		float pixels = 0f;
		float pixels2 = 0f;
		bool changed = GUI.changed;
		GUI.changed = false;
		delay = gui.FloatField("Delay:", delay);
		if (!inspector)
		{
			GUILayout.Space(pixels);
		}
		duration = gui.FloatField("Duration:", duration);
		if (duration < 0f)
		{
			repeatDelay = GUILayout.Toggle(repeatDelay, new GUIContent("Rep. Delay", "Repeats the delay on each loop iteration"));
			if (type != EZAnimation.ANIM_TYPE.AnimClip || type != EZAnimation.ANIM_TYPE.Crash || type != EZAnimation.ANIM_TYPE.CrashRotation || type != EZAnimation.ANIM_TYPE.PunchPosition || type != EZAnimation.ANIM_TYPE.PunchRotation || type != EZAnimation.ANIM_TYPE.PunchScale || type != EZAnimation.ANIM_TYPE.Shake || type != EZAnimation.ANIM_TYPE.ShakeRotation || type != EZAnimation.ANIM_TYPE.SmoothCrash)
			{
				pingPong = GUILayout.Toggle(pingPong, new GUIContent("PingPong", "Ping-Pong: Causes the animated value to go back and forth as it loops."));
			}
		}
		if (!inspector)
		{
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(pixels2);
		}
		if (type == EZAnimation.ANIM_TYPE.FadeMaterial || type == EZAnimation.ANIM_TYPE.FadeSprite || type == EZAnimation.ANIM_TYPE.FadeSpriteAlpha || type == EZAnimation.ANIM_TYPE.FadeAudio || type == EZAnimation.ANIM_TYPE.TuneAudio || type == EZAnimation.ANIM_TYPE.FadeText || type == EZAnimation.ANIM_TYPE.FadeTextAlpha || type == EZAnimation.ANIM_TYPE.Rotate || type == EZAnimation.ANIM_TYPE.Scale || type == EZAnimation.ANIM_TYPE.Translate || type == EZAnimation.ANIM_TYPE.TranslateScreen || type == EZAnimation.ANIM_TYPE.RotateEuler)
		{
			easing = (EZAnimation.EASING_TYPE)gui.EnumField("Easing:", easing);
		}
		if (!inspector)
		{
			GUILayout.Space(pixels);
		}
		if (type == EZAnimation.ANIM_TYPE.FadeMaterial || type == EZAnimation.ANIM_TYPE.FadeSprite || type == EZAnimation.ANIM_TYPE.FadeSpriteAlpha || type == EZAnimation.ANIM_TYPE.FadeAudio || type == EZAnimation.ANIM_TYPE.TuneAudio || type == EZAnimation.ANIM_TYPE.FadeText || type == EZAnimation.ANIM_TYPE.FadeTextAlpha || type == EZAnimation.ANIM_TYPE.Rotate || type == EZAnimation.ANIM_TYPE.Scale || type == EZAnimation.ANIM_TYPE.Translate || type == EZAnimation.ANIM_TYPE.TranslateScreen || type == EZAnimation.ANIM_TYPE.RotateEuler)
		{
			mode = (EZAnimation.ANIM_MODE)gui.EnumField("Mode:", mode);
		}
		if (duration < 0f && (type == EZAnimation.ANIM_TYPE.FadeMaterial || type == EZAnimation.ANIM_TYPE.FadeSprite || type == EZAnimation.ANIM_TYPE.FadeSpriteAlpha || type == EZAnimation.ANIM_TYPE.FadeAudio || type == EZAnimation.ANIM_TYPE.TuneAudio || type == EZAnimation.ANIM_TYPE.FadeText || type == EZAnimation.ANIM_TYPE.FadeTextAlpha || type == EZAnimation.ANIM_TYPE.Rotate || type == EZAnimation.ANIM_TYPE.Scale || type == EZAnimation.ANIM_TYPE.Translate || type == EZAnimation.ANIM_TYPE.TranslateScreen || type == EZAnimation.ANIM_TYPE.RotateEuler))
		{
			restartOnRepeat = GUILayout.Toggle(restartOnRepeat, new GUIContent("Restart on Loop", "Resets the starting value on each loop iteration. Set this to false if you want something like continuous movement in the same direction without going back to the starting point."));
		}
		if (!inspector)
		{
			GUILayout.EndHorizontal();
		}
		switch (type)
		{
		case EZAnimation.ANIM_TYPE.FadeSprite:
		case EZAnimation.ANIM_TYPE.FadeMaterial:
		case EZAnimation.ANIM_TYPE.FadeText:
		case EZAnimation.ANIM_TYPE.FadeSpriteAlpha:
		case EZAnimation.ANIM_TYPE.FadeTextAlpha:
			if (mode == EZAnimation.ANIM_MODE.FromTo)
			{
				color = gui.ColorField("Start Color:", color);
				color2 = gui.ColorField("End Color:", color2);
			}
			else
			{
				color = gui.ColorField("Color:", color);
			}
			break;
		case EZAnimation.ANIM_TYPE.Rotate:
		case EZAnimation.ANIM_TYPE.RotateEuler:
			if (mode == EZAnimation.ANIM_MODE.FromTo)
			{
				vec = gui.Vector3Field("Start Angles:", vec);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec = go.transform.localEulerAngles;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					go.transform.localEulerAngles = vec;
				}
				GUILayout.EndHorizontal();
				vec2 = gui.Vector3Field("End Angles:", vec2);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec2 = go.transform.localEulerAngles;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					go.transform.localEulerAngles = vec2;
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				vec = gui.Vector3Field("Angles:", vec);
				if (mode == EZAnimation.ANIM_MODE.To)
				{
					GUILayout.BeginHorizontal();
					if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
					{
						vec = go.transform.localEulerAngles;
						GUI.changed = true;
					}
					if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
					{
						go.transform.localEulerAngles = vec;
					}
					GUILayout.EndHorizontal();
				}
			}
			break;
		case EZAnimation.ANIM_TYPE.Scale:
			if (mode == EZAnimation.ANIM_MODE.FromTo)
			{
				vec = gui.Vector3Field("Start Scale:", vec);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec = go.transform.localScale;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					go.transform.localScale = vec;
				}
				GUILayout.EndHorizontal();
				vec2 = gui.Vector3Field("End Scale:", vec2);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec2 = go.transform.localScale;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					go.transform.localScale = vec2;
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				vec = gui.Vector3Field("Scale:", vec);
				if (mode == EZAnimation.ANIM_MODE.To)
				{
					GUILayout.BeginHorizontal();
					if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
					{
						vec = go.transform.localScale;
						GUI.changed = true;
					}
					if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
					{
						go.transform.localScale = vec;
					}
					GUILayout.EndHorizontal();
				}
			}
			break;
		case EZAnimation.ANIM_TYPE.Translate:
			if (mode == EZAnimation.ANIM_MODE.FromTo)
			{
				vec = gui.Vector3Field("Start Pos:", vec);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec = go.transform.localPosition;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					go.transform.localPosition = vec;
				}
				GUILayout.EndHorizontal();
				vec2 = gui.Vector3Field("End Pos:", vec2);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec2 = go.transform.localPosition;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					go.transform.localPosition = vec2;
				}
				GUILayout.EndHorizontal();
			}
			else if (mode == EZAnimation.ANIM_MODE.By)
			{
				vec = gui.Vector3Field("Vector:", vec);
			}
			else
			{
				vec = gui.Vector3Field("Pos:", vec);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec = go.transform.localPosition;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					go.transform.localPosition = vec;
				}
				GUILayout.EndHorizontal();
			}
			break;
		case EZAnimation.ANIM_TYPE.TranslateScreen:
		{
			EZScreenPlacement eZScreenPlacement = (EZScreenPlacement)go.GetComponent(typeof(EZScreenPlacement));
			if (eZScreenPlacement == null)
			{
				Debug.LogError("ERROR: A transition element of type TranslateScreen has been selected, but the object \"" + go.name + "\" does not have an EZScreenPlacement component attached.");
				break;
			}
			if (mode == EZAnimation.ANIM_MODE.FromTo)
			{
				vec = gui.Vector3Field("Start Pos:", vec);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec = eZScreenPlacement.screenPos;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					eZScreenPlacement.screenPos = vec;
					eZScreenPlacement.PositionOnScreen();
				}
				GUILayout.EndHorizontal();
				vec2 = gui.Vector3Field("End Pos:", vec2);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec2 = eZScreenPlacement.screenPos;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					eZScreenPlacement.screenPos = vec2;
					eZScreenPlacement.PositionOnScreen();
				}
				GUILayout.EndHorizontal();
			}
			else if (mode == EZAnimation.ANIM_MODE.By)
			{
				vec = gui.Vector3Field("Vector:", vec);
			}
			else
			{
				vec = gui.Vector3Field("Pos:", vec);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Use Current", "Uses the object's current value for this field")))
				{
					vec = eZScreenPlacement.screenPos;
					GUI.changed = true;
				}
				if (GUILayout.Button(new GUIContent("Set as Current", "Applies the displayed values to the current object.")))
				{
					eZScreenPlacement.screenPos = vec;
					eZScreenPlacement.PositionOnScreen();
				}
				GUILayout.EndHorizontal();
			}
			break;
		}
		case EZAnimation.ANIM_TYPE.PunchPosition:
		case EZAnimation.ANIM_TYPE.Crash:
		case EZAnimation.ANIM_TYPE.PunchScale:
		case EZAnimation.ANIM_TYPE.PunchRotation:
			vec = gui.Vector3Field("Magnitude:", vec);
			break;
		case EZAnimation.ANIM_TYPE.Shake:
		case EZAnimation.ANIM_TYPE.ShakeRotation:
			vec = gui.Vector3Field("Magnitude:", vec);
			floatVal = gui.FloatField("Oscillations:", floatVal);
			break;
		case EZAnimation.ANIM_TYPE.SmoothCrash:
		case EZAnimation.ANIM_TYPE.CrashRotation:
			vec = gui.Vector3Field("Magnitude:", vec);
			vec2 = gui.Vector3Field("Oscillations:", vec2);
			break;
		case EZAnimation.ANIM_TYPE.AnimClip:
			strVal = gui.TextField("Anim Clip:", strVal);
			if (!inspector)
			{
				GUILayout.BeginHorizontal();
				floatVal = Mathf.Clamp01(gui.FloatField("Blend Weight:", floatVal));
				GUILayout.Space(15f);
				floatVal = GUILayout.HorizontalSlider(floatVal, 0f, 1f, GUILayout.Width(200f));
				GUILayout.EndHorizontal();
			}
			else
			{
				floatVal = Mathf.Clamp01(gui.FloatField("Blend Weight:", floatVal));
			}
			break;
		case EZAnimation.ANIM_TYPE.FadeAudio:
			if (mode == EZAnimation.ANIM_MODE.FromTo)
			{
				floatVal = gui.FloatField("Start Volume:", floatVal);
				floatVal2 = gui.FloatField("End Volume:", floatVal2);
			}
			else
			{
				floatVal = gui.FloatField("Volume:", floatVal);
			}
			break;
		case EZAnimation.ANIM_TYPE.TuneAudio:
			if (mode == EZAnimation.ANIM_MODE.FromTo)
			{
				floatVal = gui.FloatField("Start pitch:", floatVal);
				floatVal2 = gui.FloatField("End pitch:", floatVal2);
			}
			else
			{
				floatVal = gui.FloatField("Pitch:", floatVal);
			}
			break;
		}
		if (GUI.changed)
		{
			m_transition.initialized = true;
		}
		GUI.changed = changed || GUI.changed;
	}
}
