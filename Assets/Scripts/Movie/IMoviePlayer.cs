public interface IMoviePlayer
{
	void StartMovie(string filename, bool canSkip);

	void UpdateMovie();

	void RenderMovie();

	void FinishMovie();

	bool IsComplete();
}
