using System;
using UnityEngine;

public class MoviePlayer : MonoBehaviour
{
	private const int MOVIE_DELAY_TICKS_COUNT = 12;

	private string _filename;

	private bool _canSkip;

	private Action _onPlayedDelegate;

	private IMoviePlayer _moviePlayer;

	private int _ticksCount;

	public static void PlayMovie(string filename, bool canSkip, Action onPlayedDelegate)
	{
		MoviePlayer moviePlayer = new GameObject("MoviePlayer").AddComponent<MoviePlayer>();
		moviePlayer._filename = filename;
		moviePlayer._canSkip = canSkip;
		moviePlayer._onPlayedDelegate = onPlayedDelegate;
	}

	private void Awake()
	{
#if UNITY_IOS || UNITY_ANDROID
		_moviePlayer = new MoviePlayerIPhone();
#else
		// PC port: Handheld.PlayFullScreenMovie does nothing on Windows, which skipped the intro
		_moviePlayer = new MoviePlayerPc();
#endif
	}

	private void Start()
	{
		_ticksCount = 0;
	}

	private void Update()
	{
		_ticksCount++;
		if (_ticksCount < 12)
		{
			return;
		}
		if (_ticksCount == 12)
		{
			_moviePlayer.StartMovie(_filename, _canSkip);
			return;
		}
		_moviePlayer.UpdateMovie();
		if (_moviePlayer.IsComplete())
		{
			Finish();
		}
	}

	private void Finish()
	{
		if (_moviePlayer != null)
		{
			_moviePlayer.FinishMovie();
		}
		UnityEngine.Object.Destroy(gameObject);
		if (_onPlayedDelegate != null)
		{
			_onPlayedDelegate();
		}
	}
}
