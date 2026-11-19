using Glu.ABTesting;

namespace Glu.DynamicContentPipeline
{
	public interface ICustomDynamicContent
	{
		void StartContentUpdate(Resolution resolution);

		void CheckForUpdates(Resolution resolution);

		float Progress { get; }

		bool IsInProgress { get; }

		bool Result { get; }

		long LastUpdateSize { get; }

		void InvalidateLastCheckReport();
	}
}
