using UnityEngine;

public class MoviePlayerTest : MonoBehaviour
{
	private void Start()
	{
		MoviePlayer.PlayMovie("GGN_Open_640x480.mov", false, null);
	}
}
