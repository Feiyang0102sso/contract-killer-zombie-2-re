using System;
using System.Linq;
using UnityEngine;

public class Raven : Animal
{
	private const float flightSpeedStart = 0.1f;

	private const float flightSpeedMax = 3.5f;

	private const float flightAcceleration = 2.5f;

	private const float takeOffRange = 4f;

	private const float desiredHeight = 8f;

	private const float angularSpeed = 180f;

	public float presenceChance = 35f;

	private Vector3 destination;

	private bool raising;

	protected override void Awake()
	{
		base.Awake();
		LogTo("Animal.Raven");
		Stay();
	}

	protected override void Start()
	{
		base.Start();
		if (!Singleton.Quality.MiscObjectsOnScene || !Util.Chance(presenceChance))
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		InitAnimations();
	}

	private void Stay()
	{
		speed = 0f;
		ChangeState(State.Staying);
	}

	private void Move()
	{
		speed = 0.15f;
		direction = transform.forward;
		ChangeState(State.Moving);
	}

	private void TakeOff()
	{
		AlertNearby();
		UnityEngine.Object.Destroy(gameObject, Util.Range(20f, 60f));
		Fly(true);
	}

	private void Fly(bool raiseUp)
	{
		raising = raiseUp;
		destination = FindWhereToFly(raiseUp);
		UnityEngine.Debug.DrawLine(Center(), destination, Color.green, 1f);
		direction = (destination - transform.position).normalized;
		speed = 0.1f;
		affectedByGravity = false;
		ChangeState(State.Running);
	}

	private Vector3 FindWhereToFly(bool raiseUp)
	{
		float a = raiseUp ? 20 : -10;
		float b = raiseUp ? 60 : 10;
		float unusedFlyAngle;
		for (int i = 0; i < 10; i++)
		{
			float num = Util.Range(a, b);
			Vector3 vector = transform.position + Vector3.up * Mathf.Sin(num * ((float)Math.PI / 180f)) * 3f + Util.RandomInCircle() * Mathf.Cos(num * ((float)Math.PI / 180f)) * 3f;
			if (!Util.AnyObstaclesSphere(Center(), vector, characterController.radius))
			{
				return vector;
			}
			UnityEngine.Debug.DrawLine(Center(), vector, Color.red, 1f);
		}
		return transform.forward + Vector3.up * 3f;
	}

	private void AlertNearby()
	{
		float unusedLocal1;
		float unusedLocal2;
		foreach (Raven item in Scene.Nearby<Raven>(transform.position, 4f))
		{
			item.Invoke("Alerted", Vector3.Distance(transform.position, item.transform.position) / 1.5f);
		}
	}

	private void Alerted()
	{
		if (state == State.Staying || state == State.Moving)
		{
			TakeOff();
			return;
		}
	}

	protected override void proceedToNextState()
	{
		if (nextState == State.Staying)
		{
			Stay();
		}
		else if (nextState == State.Moving)
		{
			Move();
		}
		else if (nextState == State.Running)
		{
			Fly(true);
		}
		else
		{
			base.proceedToNextState();
		}
	}

	protected override void Update()
	{
		float unusedLocal1;
		if (state == State.Staying || state == State.Moving)
		{
			if (Util.DoRarely(this, "take_off_check", 0.5f) && (from c in Scene.Nearby<Character>(transform.position, 4f)
				where !(c is Raven)
				select c).Count() > 0)
			{
				TakeOff();
				return;
			}
		}
		else if (state == State.Running)
		{
			if (Util.ZeroY(direction).magnitude > 0.001f)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Util.ZeroY(direction)), Time.deltaTime * 180f);
			}
			if (Util.DoRarely(this, "recheck", 1f) && Util.AnyObstaclesSphere(Center(), Center() + direction * 3.5f * 1f, characterController.radius))
			{
				Fly(true);
				return;
			}
			if (raising)
			{
				if (Util.DoRarely(this, "fly_around_check", 1.5f) && transform.position.y - Scene.Player.transform.position.y >= 8f)
				{
					Fly(false);
					return;
				}
			}
			else if (Util.ChancePerTime(25f, this, "chance_course", 2f))
			{
				Fly(false);
				return;
			}
			if (speed < 3.5f)
			{
				speed = Mathf.Min(3.5f, speed + 2.5f * Time.deltaTime);
			}
		}
		base.Update();
	}

	protected override bool CanBePained(DamageInfo info)
	{
		return false;
	}

	protected override void OnHurt(DamageInfo info)
	{
		Alerted();
		AlertNearby();
		base.OnHurt(info);
	}

	protected override void OnDying(DamageInfo info)
	{
		Singleton.PlayerData.ProgressGoal(Goal.Type.KillBirds, 1);
		AlertNearby();
		UnityEngine.Object original = Resources.Load("FxFeathers");
		GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
		gameObject.transform.position = transform.position;
		ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
		component.enableEmission = true;
		component.Play();
		UnityEngine.Object.Destroy(gameObject, 10f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void InitAnimations()
	{
		foreach (string text in new string[2] { "idle", "fly" })
		{
			animation[text].wrapMode = WrapMode.Loop;
		}
	}

	protected override void Animate(bool firstRun)
	{
		if (state == State.Staying)
		{
			if (firstRun)
			{
				PlayAnimation(Util.Range(0.7f, 1.5f), "idle");
			}
		}
		else if (state == State.Moving)
		{
			string text = "walk";
			if (firstRun)
			{
				PlayAnimation(text);
			}
			animation[text].speed = speed / 0.2f / transform.lossyScale.z;
		}
		else if (state == State.Running)
		{
			if (firstRun)
			{
				animation["take_off"].speed = 0.7f;
				PlayAnimation("take_off");
				PlayAnimationQueued("fly");
			}
			if (animation.IsPlaying("fly"))
			{
				animation["fly"].speed = 3f;
			}
		}
		else if (state == State.Pain)
		{
		}
		else if (state == State.Dying)
		{
			if (firstRun)
			{
				nextStateDelay += 0.1f;
			}
		}
		else if (state == State.Dead)
		{
		}
		else
		{
			base.Animate(firstRun);
		}
	}
}
