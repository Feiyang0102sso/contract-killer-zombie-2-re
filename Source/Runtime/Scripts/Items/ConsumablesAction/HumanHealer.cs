using UnityEngine;

public class HumanHealer : MonoBehaviour
{
	public float HealthToHeal = 100f;

	public float Duration = 1f;

	public ParticleSystem HealEffect;

	private Human _human;

	private ParticleSystem _effect;

	private void Start()
	{
		_human = GetComponent<Human>();
		_effect = Object.Instantiate(HealEffect) as ParticleSystem;
		_effect.transform.parent = transform;
		_effect.transform.localPosition = Vector3.zero;
		if (_human is Survivor)
		{
			_human.Health += HealthToHeal;
			Done();
		}
	}

	private void Update()
	{
		_human.Health += Time.deltaTime * (HealthToHeal / Duration);
		Duration -= Time.deltaTime;
		if (Duration < 0f)
		{
			Done();
		}
	}

	private void Done()
	{
		Object.Destroy(_effect.gameObject);
		Object.Destroy(this);
	}
}
