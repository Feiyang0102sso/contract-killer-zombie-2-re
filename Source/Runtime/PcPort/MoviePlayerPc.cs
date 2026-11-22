// PC bridge: full screen movie for MoviePlayer (the intro cinematic Scene_01).
// The original MoviePlayerIPhone calls Handheld.PlayFullScreenMovie, which only works on iOS / Android:
// on Windows it returns at once, so the intro was skipped. This plays the same movie with a VideoPlayer
// the way iOS did: black background, aspect fit, and with canSkip any tap (here: click, touch or key) stops it.
// The VideoPlayer decodes into a RenderTexture and MovieScreen draws it with OnGUI over everything else.
// The movie is Resources/Movies/<name>.mp4, converted from the original .mov by convert_intro_movie.py.
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class MoviePlayerPc : IMoviePlayer
{
	private const string MovieFolder = "Movies/";

	private GameObject _root;

	private VideoPlayer _videoPlayer;

	private RenderTexture _texture;

	private bool _canSkip;

	private bool _complete;

	public void StartMovie(string filename, bool canSkip)
	{
		_canSkip = canSkip;
		_complete = false;
		string resourcePath = MovieFolder + Path.GetFileNameWithoutExtension(filename);
		VideoClip clip = Resources.Load<VideoClip>(resourcePath);
		if (clip == null)
		{
			Debug.LogError("[MoviePlayerPc] movie not found: Resources/" + resourcePath);
			_complete = true;
			return;
		}
		_texture = new RenderTexture((int)clip.width, (int)clip.height, 0);
		_texture.name = "MoviePlayerPc";
		_root = new GameObject("MoviePlayerPc");
		MovieScreen screen = _root.AddComponent<MovieScreen>();
		screen.Texture = _texture;
		_videoPlayer = _root.AddComponent<VideoPlayer>();
		_videoPlayer.playOnAwake = false;
		_videoPlayer.clip = clip;
		_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		_videoPlayer.targetTexture = _texture;
		_videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
		_videoPlayer.isLooping = false;
		_videoPlayer.loopPointReached += OnMovieEnded;
		_videoPlayer.errorReceived += OnMovieError;
		_videoPlayer.Play();
		Debug.Log("[MoviePlayerPc] playing " + clip.name + " (" + clip.length.ToString("F1") + " s, " + clip.width + "x" + clip.height + ", audio tracks " + clip.audioTrackCount + "), canSkip=" + canSkip);
	}

	public void UpdateMovie()
	{
		if (_complete || !_canSkip)
		{
			return;
		}
		if (SkipPressed())
		{
			Debug.Log("[MoviePlayerPc] skipped at " + _videoPlayer.time.ToString("F1") + " s, frame " + _videoPlayer.frame);
			_complete = true;
		}
	}

	public void RenderMovie()
	{
	}

	public void FinishMovie()
	{
		if (_root != null)
		{
			Object.Destroy(_root);
			_root = null;
		}
		if (_texture != null)
		{
			_texture.Release();
			Object.Destroy(_texture);
			_texture = null;
		}
	}

	public bool IsComplete()
	{
		return _complete;
	}

	/// <summary>iOS CancelOnInput stops on a tap: here a new touch (mouse, injected test tap, touchscreen) or any key.</summary>
	private static bool SkipPressed()
	{
		if (Input.anyKeyDown)
		{
			return true;
		}
		Touch[] touches = PcInput.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			if (touches[i].phase == TouchPhase.Began)
			{
				return true;
			}
		}
		return false;
	}

	private void OnMovieEnded(VideoPlayer source)
	{
		Debug.Log("[MoviePlayerPc] finished, frame " + source.frame);
		_complete = true;
	}

	private void OnMovieError(VideoPlayer source, string message)
	{
		Debug.LogError("[MoviePlayerPc] error: " + message);
		_complete = true;
	}

	/// <summary>Draws the movie texture over the whole screen: black bars, aspect fit (iOS FullScreenMovieScalingMode.AspectFit).</summary>
	private class MovieScreen : MonoBehaviour
	{
		// Lower GUI.depth draws on top of other OnGUI overlays
		private const int GuiDepth = -1000;

		public Texture Texture;

		// Opaque black (Texture2D.blackTexture has alpha 0)
		private Texture2D _black;

		private void Awake()
		{
			_black = new Texture2D(1, 1);
			_black.SetPixel(0, 0, Color.black);
			_black.Apply();
		}

		private void OnDestroy()
		{
			Destroy(_black);
		}

		private void OnGUI()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			GUI.depth = GuiDepth;
			Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
			GUI.DrawTexture(screenRect, _black, ScaleMode.StretchToFill);
			GUI.DrawTexture(screenRect, Texture, ScaleMode.ScaleToFit);
		}
	}
}
