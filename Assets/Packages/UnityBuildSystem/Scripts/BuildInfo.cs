using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Glu.UnityBuildSystem
{
	public class BuildInfo
	{
		private static string _buildTag;

		public static string buildTag
		{
			get
			{
				if (string.IsNullOrEmpty(_buildTag))
				{
					TextAsset textAsset = Resources.Load("build_tag") as TextAsset;
					if (textAsset != null)
					{
						_buildTag = textAsset.text.Trim();
					}
					else
					{
						_buildTag = "NOT_TAGGED";
					}
				}
				return _buildTag;
			}
		}

		public static string bundleId
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				GetBundleId(stringBuilder, stringBuilder.Capacity);
				return stringBuilder.ToString();
			}
		}

		public static string bundleVersion
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				GetBundleVersion(stringBuilder, stringBuilder.Capacity);
				return stringBuilder.ToString();
			}
		}

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void GetBundleId(StringBuilder sb, int size);

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void GetBundleVersion(StringBuilder sb, int size);
	}
}
