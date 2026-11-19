using UnityEngine;

public class HumanInHelicopter : Human
{
	protected override void Awake()
	{
		base.Awake();
		GameObject gameObject = Util.Find("InHelicopter");
		string text = "wallis_helicopter";
		gameObject.animation[text].wrapMode = WrapMode.Loop;
		gameObject.animation.Play(text);
		showHealth = ShowHealth.Never;
	}

	protected override void Start()
	{
		base.Start();
		ChangeState(State.Staying);
	}

	protected override void OnHurt(DamageInfo info)
	{
		if (PainSound != null && PainSound.Length > 0 && Util.DoRarely(this, "SURUGADOI", 0.33f))
		{
			SoundManager.PlayExclusive(PainSound[Random.Range(0, PainSound.Length)], transform.position + Util.RandomInCircle() * 1.5f, 10f, 50f, false);
		}
	}

	protected override void Animate(bool firstRun)
	{
		if (state == State.Staying)
		{
		}
		else
		{
			base.Animate(firstRun);
		}
	}
}
