using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{
	private class C : IEnumerable
	{
		public void Add(int a, int b)
		{
		}

		public IEnumerator GetEnumerator()
		{
			yield break;
		}
	}

	private void Start()
	{
	}
}
