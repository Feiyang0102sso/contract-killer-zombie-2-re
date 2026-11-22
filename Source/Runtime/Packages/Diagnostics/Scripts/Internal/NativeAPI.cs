using System.Runtime.InteropServices;

namespace Glu.Diagnostics.Internal
{
	public static class NativeAPI
	{
		private const string __importName = "__Internal";

#if UNITY_IOS
		[DllImport("__Internal")]
		public static extern void Glu_Diagnostics_Abort();
#else
		public static void Glu_Diagnostics_Abort()
		{
			NonIosNative.Stub("Glu_Diagnostics_Abort");
		}
#endif
	}
}
