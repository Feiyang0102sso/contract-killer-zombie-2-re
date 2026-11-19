using System.Runtime.InteropServices;

namespace Glu.Diagnostics.Internal
{
	public static class NativeAPI
	{
		private const string __importName = "__Internal";

		[DllImport("__Internal")]
		public static extern void Glu_Diagnostics_Abort();
	}
}
