using UnityEngine;

public class HintArrow : MonoBehaviour
{
	private const float OscillFreq = 12f;

	private const float OscillRange = 0.5f;

	public Vector3 OriginalPosition { get; private set; }

	public static GameObject Create(GameObject parent)
	{
		Renderer renderer = (!(!parent.renderer) ? parent.renderer : parent.GetComponentInChildren<Renderer>());
		GameObject gameObject = Util.CreateGameObject("HintArrow");
		gameObject.SetParent(parent);
		gameObject.transform.position = (renderer ? (Util.ZeroY(renderer.transform.position) + Vector3.up * renderer.bounds.max.y) : parent.transform.position);
		gameObject.transform.rotation = Quaternion.identity;
		Vector3 lossyScale = gameObject.transform.lossyScale;
		gameObject.transform.localScale = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
		return gameObject;
	}

	private void Start()
	{
		OriginalPosition = transform.position;
		Singleton.Visual.ShowOutline(gameObject, Color.white, 0);
	}

	private void Update()
	{
		Util.RotateToCamera(transform);
		float num = (Mathf.Sin(TimeControl.GameTime * 12f) + 1f) * 0.5f * 0.5f;
		transform.position = OriginalPosition + Vector3.up * num;
	}

	public void Show(bool show)
	{
		gameObject.SetActiveRecursively(show);
	}
}
