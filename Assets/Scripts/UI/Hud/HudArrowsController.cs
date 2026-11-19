using Glu.Localization;
using UnityEngine;

public class HudArrowsController : MonoBehaviourAndLog
{
	private Player _player;

	private HudController _hudController;

	public void RegisterPlayer(Player player)
	{
		_player = player;
	}

	private void Awake()
	{
		LogTo("Hud.HudArrowsController");
	}

	private void Start()
	{
		_hudController = GetComponent<HudController>();
	}

	private void Update()
	{
		if (_player == null)
		{
			return;
		}
		if (Scene.Spawner == null)
		{
			return;
		}
		if (Util.DoRarelyReal(this, "refind_danger", 0.3f))
		{
			Camera camera = Scene.Camera.camera;
			Unit meleeTarget = null;
			float num = 180f;
			bool flag = false;
			bool flag2 = false;
			float num2 = camera.fov / 2f * camera.aspect;
			foreach (Unit enemy in Scene.Enemies)
			{
				float num3 = Mathf.Abs(HudArrow.Angle(camera.transform.position, enemy.Eyes()));
				float magnitude = (enemy.transform.position - _player.transform.position).magnitude;
				float unusedDangerDistance;
				float unusedDangerAngle;
				if (num3 < num2)
				{
					if (magnitude < _player.meleeAttackDistance + _player.Radius() && num3 < num && enemy.CanBeHurt())
					{
						num = num3;
						meleeTarget = enemy;
					}
					float num4 = Mathf.Abs(num3 - num2) / num2 * 0.70000005f + 0.4f;
					if (magnitude < _player.Inventory.Weapon.MaxAutofireDistance * num4)
					{
						if (!flag && !Util.AnyObstacles(camera.transform.position, enemy.Eyes()))
						{
							flag = true;
						}
					}
					else if (num3 <= 2f && !flag2 && !Util.AnyObstacles(camera.transform.position, enemy.Eyes()))
					{
						flag2 = true;
					}
				}
			}
			_hudController.FastAimMode = flag;
			_player.MeleeTarget = meleeTarget;
			if (flag2 && !flag && !_player.Inventory.Weapon.IsMelee())
			{
				_hudController.ShowCursorInfo(Strings.GetString("out of range"));
			}
		}
	}
}
