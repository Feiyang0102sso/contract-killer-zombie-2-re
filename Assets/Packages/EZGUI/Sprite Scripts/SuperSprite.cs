using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SuperSpriteAnimElement
{
	public AutoSpriteBase sprite;

	public string animName;

	[HideInInspector]
	public UVAnimation anim;

	public void Init()
	{
		bool flag = false;
		if (sprite != null)
		{
			if (!sprite.gameObject.active)
			{
				flag = true;
				sprite.gameObject.active = true;
			}
			anim = sprite.GetAnim(animName);
			if (anim == null)
			{
				Debug.LogError("SuperSprite error: No animation by the name of \"" + animName + "\" was found on sprite \"" + sprite.name + "\". Please verify the spelling and capitalization of the name, including any extra spaces, etc.");
			}
			if (flag)
			{
				sprite.gameObject.active = false;
			}
		}
	}

	public void Play()
	{
		sprite.PlayAnim(anim);
	}

	public void PlayInReverse()
	{
		sprite.PlayAnimInReverse(anim);
	}
}

[Serializable]
public class SuperSpriteAnim
{
	public enum ANIM_END_ACTION
	{
		Do_Nothing,
		Play_Default_Anim,
		Deactivate,
		Destroy
	}

	public delegate void AnimCompletedDelegate(SuperSpriteAnim anim);

	[HideInInspector]
	public int index;

	public string name;

	public SuperSpriteAnimElement[] spriteAnims = new SuperSpriteAnimElement[0];

	public int loopCycles;

	public bool pingPong;

	public ANIM_END_ACTION onAnimEnd;

	public bool deactivateNonPlaying;

	public bool deactivateRecursively;

	protected int curAnim;

	protected int stepDir = 1;

	protected int numLoops;

	protected bool isRunning;

	private AnimCompletedDelegate endDelegate;

	public void Init(int idx, AnimCompletedDelegate del, SpriteBase.AnimFrameDelegate frameDel)
	{
		endDelegate = del;
		index = idx;
		List<SuperSpriteAnimElement> list = new List<SuperSpriteAnimElement>();
		for (int i = 0; i < spriteAnims.Length; i++)
		{
			if (spriteAnims[i] != null && spriteAnims[i].sprite != null)
			{
				spriteAnims[i].Init();
				list.Add(spriteAnims[i]);
				if (frameDel != null)
				{
					spriteAnims[i].sprite.SetAnimFrameDelegate(frameDel);
				}
				HideSprite(spriteAnims[i].sprite, true);
			}
		}
		spriteAnims = list.ToArray();
	}

	public void SetAnimFrameDelegate(SpriteBase.AnimFrameDelegate frameDel)
	{
		for (int i = 0; i < spriteAnims.Length; i++)
		{
			if (spriteAnims[i] != null && spriteAnims[i].sprite != null)
			{
				spriteAnims[i].sprite.SetAnimFrameDelegate(frameDel);
			}
		}
	}

	private void AnimFinished(SpriteBase sp)
	{
		if (curAnim + stepDir >= spriteAnims.Length || curAnim + stepDir < 0)
		{
			if (stepDir > 0 && pingPong)
			{
				stepDir = -1;
				((AutoSpriteBase)sp).PlayAnimInReverse(spriteAnims[curAnim].anim, spriteAnims[curAnim].anim.GetFrameCount() - 2);
				return;
			}
			if (numLoops + 1 > loopCycles && loopCycles != -1)
			{
				isRunning = false;
				sp.SetAnimCompleteDelegate(null);
				if (endDelegate != null)
				{
					endDelegate(this);
				}
				return;
			}
			numLoops++;
			if (pingPong)
			{
				spriteAnims[curAnim].sprite.PlayAnim(spriteAnims[curAnim].anim, 1);
				stepDir *= -1;
				return;
			}
			HideSprite(sp, true);
			sp.SetAnimCompleteDelegate(null);
			curAnim = 0;
		}
		else
		{
			sp.SetAnimCompleteDelegate(null);
			HideSprite(sp, true);
			curAnim += stepDir;
		}
		HideSprite(spriteAnims[curAnim].sprite, false);
		spriteAnims[curAnim].sprite.SetAnimCompleteDelegate(AnimFinished);
		if (stepDir > 0)
		{
			spriteAnims[curAnim].Play();
		}
		else
		{
			spriteAnims[curAnim].PlayInReverse();
		}
	}

	public void Reset()
	{
		Stop();
		curAnim = 0;
		stepDir = 1;
		numLoops = 0;
		for (int i = 1; i < spriteAnims.Length; i++)
		{
			HideSprite(spriteAnims[i].sprite, true);
		}
	}

	public void Play()
	{
		isRunning = true;
		spriteAnims[curAnim].sprite.SetAnimCompleteDelegate(AnimFinished);
		HideSprite(spriteAnims[curAnim].sprite, false);
		spriteAnims[curAnim].Play();
	}

	public void Stop()
	{
		isRunning = false;
		spriteAnims[curAnim].sprite.StopAnim();
		spriteAnims[curAnim].sprite.SetAnimCompleteDelegate(null);
	}

	public void Pause()
	{
		isRunning = false;
		spriteAnims[curAnim].sprite.PauseAnim();
	}

	public void Unpause()
	{
		isRunning = true;
		spriteAnims[curAnim].sprite.UnpauseAnim();
	}

	public bool IsRunning
	{
		get
		{
			return isRunning;
		}
	}

	public SpriteBase CurrentSprite
	{
		get
		{
			if (curAnim < 0 || curAnim >= spriteAnims.Length)
			{
				return null;
			}
			return spriteAnims[curAnim].sprite;
		}
	}

	public void Hide(bool tf)
	{
		if (!(curAnim >= 0 && curAnim < spriteAnims.Length))
		{
			return;
		}
		HideSprite(spriteAnims[curAnim].sprite, tf);
	}

	public bool IsHidden()
	{
		if (curAnim < 0 || curAnim >= spriteAnims.Length)
		{
			return false;
		}
		return spriteAnims[curAnim].sprite.IsHidden();
	}

	protected void HideSprite(SpriteBase sp, bool tf)
	{
		if (deactivateNonPlaying)
		{
			if (deactivateRecursively)
			{
				sp.gameObject.SetActiveRecursively(!tf);
			}
			else
			{
				sp.gameObject.active = !tf;
			}
		}
		else
		{
			sp.Hide(tf);
		}
	}

	public void Delete()
	{
		for (int i = 0; i < spriteAnims.Length; i++)
		{
			if (spriteAnims[i].sprite != null)
			{
				spriteAnims[i].sprite.Delete();
				UnityEngine.Object.Destroy(spriteAnims[i].sprite);
			}
		}
	}
}

[Serializable]
public class SuperSprite : MonoBehaviour
{
	public delegate void AnimCompleteDelegate(SuperSprite sprite);

	public bool playDefaultAnimOnStart;

	public int defaultAnim;

	public SuperSpriteAnim[] animations = new SuperSpriteAnim[0];

	protected SuperSpriteAnim curAnim;

	protected bool animating;

	protected AnimCompleteDelegate animCompleteDelegate;

	protected SpriteBase.AnimFrameDelegate animFrameDelegate;

	protected bool m_started;

	public void Start()
	{
		if (m_started)
		{
			return;
		}
		m_started = true;
		for (int i = 0; i < animations.Length; i++)
		{
			if (animations[i] != null)
			{
				animations[i].Init(i, AnimFinished, animFrameDelegate);
			}
		}
		if (playDefaultAnimOnStart)
		{
			PlayAnim(animations[defaultAnim]);
		}
	}

	public void PlayAnim(SuperSpriteAnim anim)
	{
		if (!m_started)
		{
			Start();
		}
		if (curAnim != null)
		{
			curAnim.Hide(true);
		}
		curAnim = anim;
		curAnim.Reset();
		animating = true;
		anim.Play();
	}

	public void PlayAnim(int index)
	{
		if (!(index >= 0 && index < animations.Length))
		{
			return;
		}
		PlayAnim(animations[index]);
	}

	public void PlayAnim(string anim)
	{
		for (int i = 0; i < animations.Length; i++)
		{
			if (animations[i].name == anim)
			{
				PlayAnim(animations[i]);
				return;
			}
		}
	}

	public void DoAnim(SuperSpriteAnim anim)
	{
		if ((curAnim != anim) | !animating)
		{
			PlayAnim(anim);
		}
	}

	public void DoAnim(int index)
	{
		if (curAnim == null)
		{
			PlayAnim(index);
		}
		else if (curAnim.index != index || !animating)
		{
			PlayAnim(index);
		}
	}

	public void DoAnim(string name)
	{
		if (curAnim == null)
		{
			PlayAnim(name);
		}
		else if (curAnim.name != name || !animating)
		{
			PlayAnim(name);
		}
	}

	public void StopAnim()
	{
		if (curAnim != null)
		{
			curAnim.Stop();
		}
		animating = false;
	}

	public void PauseAnim()
	{
		if (curAnim != null)
		{
			curAnim.Pause();
		}
		animating = false;
	}

	public void UnpauseAnim()
	{
		if (curAnim != null)
		{
			curAnim.Unpause();
			animating = true;
		}
	}

	public void Hide(bool tf)
	{
		if (!(curAnim == null))
		{
			curAnim.Hide(tf);
			if (!tf)
			{
				if (animating)
				{
					curAnim.Pause();
				}
			}
			else if (animating)
			{
				curAnim.Unpause();
			}
		}
	}

	public bool IsHidden()
	{
		if (curAnim == null)
		{
			return false;
		}
		return curAnim.IsHidden();
	}

	public SuperSpriteAnim GetCurAnim()
	{
		return curAnim;
	}

	public SpriteRoot CurrentSprite
	{
		get
		{
			if (curAnim == null)
			{
				return null;
			}
			return curAnim.CurrentSprite;
		}
	}

	public SuperSpriteAnim GetAnim(int index)
	{
		if (index < 0 || index >= animations.Length)
		{
			return null;
		}
		return animations[index];
	}

	public SuperSpriteAnim GetAnim(string name)
	{
		for (int i = 0; i < animations.Length; i++)
		{
			if (animations[i].name == name)
			{
				return animations[i];
			}
		}
		return null;
	}

	public bool IsAnimating()
	{
		return animating;
	}

	protected void AnimFinished(SuperSpriteAnim anim)
	{
		animating = false;
		if (animCompleteDelegate != null)
		{
			animCompleteDelegate(this);
		}
		if (curAnim != null)
		{
			switch (curAnim.onAnimEnd)
			{
			case SuperSpriteAnim.ANIM_END_ACTION.Play_Default_Anim:
				PlayAnim(defaultAnim);
				break;
			case SuperSpriteAnim.ANIM_END_ACTION.Deactivate:
				gameObject.SetActiveRecursively(false);
				break;
			case SuperSpriteAnim.ANIM_END_ACTION.Destroy:
			{
				for (int i = 0; i < animations.Length; i++)
				{
					animations[i].Delete();
				}
				UnityEngine.Object.Destroy(gameObject);
				break;
			}
			default:
				break;
			}
		}
	}

	public void SetAnimCompleteDelegate(AnimCompleteDelegate del)
	{
		animCompleteDelegate = del;
	}

	public void SetAnimFrameDelegate(SpriteBase.AnimFrameDelegate del)
	{
		animFrameDelegate = del;
		for (int i = 0; i < animations.Length; i++)
		{
			animations[i].SetAnimFrameDelegate(animFrameDelegate);
		}
	}
}
