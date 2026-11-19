using UnityEngine;

public class MoviePlayerIPhone : IMoviePlayer
{
	public void StartMovie(string filename, bool canSkip)
	{
		Handheld.PlayFullScreenMovie("Movies/" + filename, Color.black, canSkip ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit);
	}

	public void UpdateMovie()
	{
	}

	public void RenderMovie()
	{
	}

	public void FinishMovie()
	{
	}

	public bool IsComplete()
	{
		return true;
	}
}
