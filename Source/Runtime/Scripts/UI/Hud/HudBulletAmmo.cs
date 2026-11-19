using UnityEngine;

public class HudBulletAmmo : MonoBehaviour
{
	private const int BulletCount = 5;

	public PackedSprite Bullet;

	public SpriteText CountText;

	private static Color BulletFilledColor = new Color(1f, 1f, 0f, 1f);

	private static Color BulletEmptyColor = new Color(1f, 1f, 1f, 1f);

	private Weapon _weapon;

	private PackedSprite[] _bullets = new PackedSprite[5];

	private void Awake()
	{
		Transform transform = new GameObject("Bullets").transform;
		transform.parent = base.transform;
		transform.localPosition = Vector3.zero;
		for (int i = 0; i < 5; i++)
		{
			float unusedBulletWidth;
			_bullets[i] = i > 0 ? (Object.Instantiate(Bullet.gameObject) as GameObject).GetComponent<PackedSprite>() : Bullet;
			_bullets[i].transform.parent = transform;
			_bullets[i].transform.localPosition = new Vector3((float)(-i) * 1.1f, 0f, -1f);
		}
	}

	public void Init(Weapon weapon)
	{
		_weapon = weapon;
		Refresh();
	}

	public void Refresh()
	{
		if (_weapon == null)
		{
			return;
		}
		float num = _weapon.GetAmmoLeftPercent() + 1f;
		for (int i = 0; i < 5; i++)
		{
			bool flag = i + 1 <= Mathf.RoundToInt(num * 0.01f * 5f);
			_bullets[i].Color = flag ? BulletFilledColor : BulletEmptyColor;
		}
		CountText.Text = "x" + _weapon.GetTotalAmmo();
	}
}
