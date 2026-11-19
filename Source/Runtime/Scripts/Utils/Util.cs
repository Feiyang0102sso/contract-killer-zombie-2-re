using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Glu.AssetBundles;
using Glu.UnityBuildSystem;
using UnityEngine;
using UnityEngine.AI;

internal class UtilLog : LoggerSingleton<UtilLog>
{
	public UtilLog()
	{
		LoggerSingleton<UtilLog>.SetLoggerName("Other.Util");
	}
}

public static class Util
{
	public class NaturalComparer : IComparer<string>
	{
		public int Compare(string a, string b)
		{
			return CompareNatural(a, b);
		}
	}

	private static List<int> numbers = null;

	private static Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

	private static Dictionary<KeyValuePair<GameObject, string>, GameObject> gameObjectChildren = new Dictionary<KeyValuePair<GameObject, string>, GameObject>();

	public static Dictionary<object, Dictionary<string, float>> rarelyMap = new Dictionary<object, Dictionary<string, float>>();

	public static Rect Area(float centerX, float centerY, float sizeX, float sizeY, Rect parent)
	{
		return new Rect(parent.x + parent.width * (centerX - sizeX / 2f) / 100f, parent.y + parent.height * (centerY - sizeY / 2f) / 100f, parent.width * sizeX / 100f, parent.height * sizeY / 100f);
	}

	public static Rect Area(float centerX, float centerY, float sizeX, float sizeY)
	{
		return Area(centerX, centerY, sizeX, sizeY, new Rect(0f, 0f, Screen.width, Screen.height));
	}

	public static void LabelInEditor(Vector3 position, string text)
	{
	}

	public static float ZeroInfinityToZeroOne(float x)
	{
		return 1f - 1f / (x + 1f);
	}

	public static Vector3 XyzToXz0(Vector3 vector)
	{
		return new Vector3(vector.x, vector.z, 0f);
	}

	public static Vector2 XyzToXz(Vector3 vector)
	{
		return new Vector2(vector.x, vector.z);
	}

	public static Vector3 ZeroY(Vector3 vector)
	{
		Vector3 result = vector;
		result.y = 0f;
		return result;
	}

	public static Quaternion OnlyY(Quaternion rotation)
	{
		return Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
	}

	public static int RoundToIntProb(float number)
	{
		return Mathf.FloorToInt(number) + (Chance(100f * (number - (float)Mathf.FloorToInt(number))) ? 1 : 0);
	}

	public static Quaternion MatrixToQuaternion(Matrix4x4 m)
	{
		Quaternion result = new Quaternion();
		result.w = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] + m[1, 1] + m[2, 2])) / 2f;
		result.x = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] - m[1, 1] - m[2, 2])) / 2f;
		result.y = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] + m[1, 1] - m[2, 2])) / 2f;
		result.z = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] - m[1, 1] + m[2, 2])) / 2f;
		result.x *= Mathf.Sign(result.x * (m[2, 1] - m[1, 2]));
		result.y *= Mathf.Sign(result.y * (m[0, 2] - m[2, 0]));
		result.z *= Mathf.Sign(result.z * (m[1, 0] - m[0, 1]));
		return result;
	}

	public static bool InRange<T>(this T n, T a, T b) where T : IComparable
	{
		return n.CompareTo(a) > 0 && n.CompareTo(b) < 0;
	}

	public static bool InSegment<T>(this T n, T a, T b) where T : IComparable
	{
		return n.CompareTo(a) >= 0 && n.CompareTo(b) <= 0;
	}

	public static float FractionInInterval(float value, float a, float b)
	{
		return Mathf.Clamp01((value - a) / (b - a));
	}

	public static bool IsFibonacci(int n)
	{
		float num = Mathf.Sqrt(5 * n * n + 4);
		float num2 = Mathf.Sqrt(5 * n * n - 4);
		return Mathf.Round(num) == num || Mathf.Round(num2) == num2;
	}

	public static int FibonacciNumber(int number)
	{
		int i;
		if (numbers == null)
		{
			List<int> list = new List<int>();
			list.Add(0);
			list.Add(1);
			numbers = list;
		}
		if (number >= numbers.Count)
		{
			for (i = numbers.Count; i <= number; i++)
			{
				numbers.Add(numbers[i - 2] + numbers[i - 1]);
			}
		}
		return numbers[number];
	}

	public static bool AnyObstacles(Vector3 from, Vector3 to)
	{
		Vector3 direction = to - from;
		return Physics.Raycast(from, direction, direction.magnitude, Const.LayerMaskObstacles);
	}

	public static bool AnyObstaclesSphere(Vector3 from, Vector3 to, float radius)
	{
		Vector3 direction = to - from;
		return Physics.SphereCast(new Ray(from, direction), radius, direction.magnitude, Const.LayerMaskObstacles);
	}

	public static bool AnyNavObstacles(Vector3 from, Vector3 to)
	{
		UnityEngine.AI.NavMeshHit hit;
		UnityEngine.AI.NavMesh.Raycast(from, to, out hit, -1);
		return hit.hit;
	}

	public static void ExplosionDealsDamage(float damageInCenter, float damageOnBorder, float damageRandomness, Vector3 center, float radius, Character source, Consumable consumable, float impulse, Func<Character, bool> charFilter)
	{
		foreach (Character item in Scene.Nearby<Character>(center, radius))
		{
			if (charFilter != null && !charFilter(item))
			{
				continue;
			}
			float num = ZeroY(item.Center() - center).magnitude - item.Radius();
			float num2 = Mathf.Clamp(num / radius, 0f, 1f);
			float number = (1f - num2) * damageInCenter + num2 * damageOnBorder;
			number = ApplyRandomness(number, damageRandomness);
			float knockback;
			item.OnDamage(new ExplosionInfo(number, DamageInfo.Type.Explosion, source, impulse * (item.Center() - center).normalized, 80f, 50f, 100f, center, radius, consumable), num2 * radius / 20f);
		}
	}

	public static T RandomFrom<T>(IEnumerable<T> array)
	{
		return RandomFrom(array.ToArray());
	}

	public static T RandomFrom<T>(params T[] array)
	{
		return array[UnityEngine.Random.Range(0, array.Length)];
	}

	public static T RandomFrom<T>(int seed, params T[] array)
	{
		return array[seed % array.Length];
	}

	public static KeyValuePair<K, T> RandomFromDictionary<K, T>(Dictionary<K, T> dictionary)
	{
		int num = UnityEngine.Random.Range(0, dictionary.Count);
		int num2 = 0;
		foreach (KeyValuePair<K, T> item in dictionary)
		{
			if (num2++ == num)
			{
				return item;
			}
		}
		return dictionary.GetEnumerator().Current;
	}

	public static K RandomKeyFromDictionary<K, T>(Dictionary<K, T> dictionary)
	{
		return RandomFromDictionary(dictionary).Key;
	}

	public static T RandomValueFromDictionary<K, T>(Dictionary<K, T> dictionary)
	{
		return RandomFromDictionary(dictionary).Value;
	}

	public static T RandomByWeights<T>(Dictionary<T, float> dictionary)
	{
		float num = 0f;
		foreach (KeyValuePair<T, float> item in dictionary)
		{
			num += item.Value;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		foreach (KeyValuePair<T, float> item2 in dictionary)
		{
			num2 -= item2.Value;
			if (num2 <= 0f)
			{
				return item2.Key;
			}
		}
		return default(T);
	}

	public static void Shuffle<T>(this List<T> array)
	{
		for (int num = array.Count; num > 1; num--)
		{
			int index = UnityEngine.Random.Range(0, num);
			T value = array[index];
			array[index] = array[num - 1];
			array[num - 1] = value;
		}
	}

	public static List<T> Shuffled<T>(this List<T> array)
	{
		List<T> list = new List<T>(array);
		list.Shuffle();
		return list;
	}

	public static bool Chance(float percent)
	{
		return UnityEngine.Random.Range(0f, 99.9f) < percent;
	}

	public static bool Chance(float percent, int seed)
	{
		return (float)(seed % 100) < percent;
	}

	public static bool ChancePerTime(float percent, object obj, string name, float recheckDelay)
	{
		return DoRarely(obj, name, recheckDelay) && Chance(percent);
	}

	public static Vector3 InCircle(float angle)
	{
		return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
	}

	public static Vector3 RandomInCircle()
	{
		return InCircle(UnityEngine.Random.Range(0f, 360f));
	}

	public static float ApplyRandomness(float number, float randomness)
	{
		return number * (1f + UnityEngine.Random.Range(-1f, 1f) * randomness);
	}

	public static float Range(float a, float b)
	{
		return UnityEngine.Random.Range(a, b);
	}

	public static void ModifyValues(GameObject obj, List<FieldModification> modifiers)
	{
		modifiers.ForEach((FieldModification m) =>
		{
			ModifyValue(obj, m.component, m.field, m.value);
		});
	}

	public static void ModifyValuesOld(GameObject obj, Dictionary<string, Dictionary<string, string>> modifiers)
	{
		foreach (KeyValuePair<string, Dictionary<string, string>> modifier in modifiers)
		{
			string key = modifier.Key;
			foreach (KeyValuePair<string, string> item in modifier.Value)
			{
				string key2 = item.Key;
				string value = item.Value;
				ModifyValue(obj, key, key2, value);
			}
		}
	}

	public static void ModifyValue(GameObject obj, string component, string field, string value)
	{
		Component component2 = obj.GetComponent(component);
		if (!component2)
		{
			return;
		}
		FieldInfo field2 = component2.GetType().GetField(field);
		if (field2 == null)
		{
			return;
		}
		if (value == string.Empty && (field2.FieldType == typeof(int) || field2.FieldType == typeof(float)))
		{
			value = "0";
		}
		try
		{
			field2.SetValue(component2, Convert.ChangeType(value, field2.FieldType));
		}
		catch (Exception e)
		{
			return;
		}
	}

	public static void ModifyValues(GameObject obj, Dictionary<string, Dictionary<string, string>> modifiers)
	{
		foreach (KeyValuePair<string, Dictionary<string, string>> modifier in modifiers)
		{
			string key = modifier.Key;
			Component component = obj.GetComponent(key);
			if (!component)
			{
				return;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>(modifier.Value);
			Type type = component.GetType();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				string key2 = item.Key;
				string text = item.Value;
				FieldInfo field = type.GetField(key2);
				if (field == null)
				{
					return;
				}
				if (text == string.Empty && (field.FieldType == typeof(int) || field.FieldType == typeof(float)))
				{
					text = "0";
				}
				try
				{
					field.SetValue(component, Convert.ChangeType(text, field.FieldType));
				}
				catch (Exception e)
				{
					return;
				}
			}
		}
	}

	public static Transform FindTransform(Transform parent, string name, bool findInactive)
	{
		if (parent.name == name)
		{
			return parent;
		}
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>(findInactive);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	public static GameObject FindObject(GameObject parent, string name)
	{
		if (null == parent)
		{
			return null;
		}
		Transform transform = FindTransform(parent.transform, name, true);
		if (null == transform)
		{
			return null;
		}
		return transform.gameObject;
	}

	public static GameObject FindObjectWithTagInHierarchy(GameObject obj, string tagName)
	{
		if (obj.tag == tagName)
		{
			return obj;
		}
		GameObject gameObject = null;
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			gameObject = FindObjectWithTagInHierarchy(obj.transform.GetChild(i).gameObject, tagName);
			if ((bool)gameObject)
			{
				break;
			}
		}
		return gameObject;
	}

	public static Transform FindTransform(Transform parent, string name)
	{
		return FindTransform(parent, name, false);
	}

	public static TComponent FindComponent<TComponent>(Transform parent, string name, bool findInactive) where TComponent : Component
	{
		Transform transform = FindTransform(parent, name, findInactive);
		if (transform != null)
		{
			return transform.GetComponent<TComponent>();
		}
		return (TComponent)null;
	}

	public static TComponent FindComponent<TComponent>(Transform parent, string name) where TComponent : Component
	{
		return FindComponent<TComponent>(parent, name, false);
	}

	public static GameObject CreateGameObject(string resourceName)
	{
		return CreateGameObject(resourceName, false);
	}

	public static GameObject CreateGameObject(string resourceName, bool bundles)
	{
		GameObject gameObject;
		if (bundles)
		{
			gameObject = AssetBundles.Load(resourceName) as GameObject;
		}
		else
		{
			gameObject = Resources.Load(resourceName) as GameObject;
		}
		return gameObject ? UnityEngine.Object.Instantiate(gameObject) as GameObject : null;
	}

	public static GameObject CreateGameObject(string fileName, string path)
	{
		return CreateGameObject(path + "/" + fileName + ".prefab", true);
	}

	public static Texture2D CreateTexture(Color color)
	{
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixel(0, 0, color);
		texture2D.Apply();
		texture2D.hideFlags = HideFlags.DontSave;
		return texture2D;
	}

	public static string GetFileNameWithBuildTag(string fileName)
	{
		return string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(fileName), BuildInfo.buildTag, Path.GetExtension(fileName));
	}

	public static byte[] LoadConfigFile(string path)
	{
		byte[] array = null;
		try
		{
			array = StorageManager.ReadFromLocation(Path.Combine(StorageManager.TemporaryCachePath, GetFileNameWithBuildTag(Path.GetFileName(path))));
		}
		catch (Exception)
		{
			UnityEngine.Object obj = AssetBundles.Load(path);
			if (obj == null)
			{
				return null;
			}
			TextAsset textAsset = obj as TextAsset;
			array = textAsset.bytes;
		}
		return array;
	}

	public static void ResetFind()
	{
		gameObjects.Clear();
	}

	public static GameObject Find(string name)
	{
		GameObject value;
		if (!gameObjects.TryGetValue(name, out value))
		{
			GameObject gameObject = GameObject.Find(name);
			if (!gameObject)
			{
				return null;
			}
			gameObjects[name] = gameObject;
			return gameObject;
		}
		if (!value)
		{
			gameObjects.Remove(name);
			return null;
		}
		return value;
	}

	public static GameObject FindChild(this GameObject parent, string name)
	{
		Transform transform = parent.transform.Find(name);
		return transform ? transform.gameObject : null;
	}

	public static GameObject FindChild(this MonoBehaviour behaviour, string name)
	{
		return behaviour.gameObject.FindChild(name);
	}

	public static GameObject GetParent(this GameObject obj)
	{
		Transform parent = obj.transform.parent;
		return parent ? parent.gameObject : null;
	}

	public static void SetParent(this GameObject obj, GameObject newParent)
	{
		obj.transform.parent = (newParent ? newParent.transform : null);
	}

	public static TComponent FindComponentUpwards<TComponent>(this GameObject obj) where TComponent : Component
	{
		GameObject gameObject = obj;
		while (gameObject != null)
		{
			TComponent component = gameObject.GetComponent<TComponent>();
			if (component != null)
			{
				return component;
			}
			gameObject = gameObject.GetParent();
		}
		return (TComponent)null;
	}

	public static TComponent FindComponentInParents<TComponent>(this GameObject obj) where TComponent : Component
	{
		return obj.GetParent().FindComponentUpwards<TComponent>();
	}

	public static void ResetFindChild()
	{
		gameObjectChildren.Clear();
	}

	public static GameObject QuickFindChild(this GameObject parent, string name)
	{
		KeyValuePair<GameObject, string> key = new KeyValuePair<GameObject, string>(parent, name);
		GameObject value;
		if (!gameObjectChildren.TryGetValue(key, out value))
		{
			GameObject gameObject = parent.FindChild(name);
			if (!gameObject)
			{
				return null;
			}
			gameObjectChildren[key] = gameObject;
			return gameObject;
		}
		if (!value)
		{
			gameObjectChildren.Remove(key);
			return null;
		}
		return value;
	}

	public static GameObject QuickFindChild(this MonoBehaviour behaviour, string name)
	{
		return behaviour.gameObject.QuickFindChild(name);
	}

	public static void DoForHierarchy(GameObject root, bool doForRoot, Func<GameObject, bool> action)
	{
		if (doForRoot)
		{
			action(root);
		}
		foreach (Transform item in root.transform)
		{
			if (action(item.gameObject))
			{
				DoForHierarchy(item.gameObject, false, action);
			}
		}
	}

	public static void DoForHierarchy(GameObject root, Func<GameObject, bool> action)
	{
		DoForHierarchy(root, true, action);
	}

	public static GameObject FindCustomChild(this GameObject root, Func<GameObject, bool> checkNode)
	{
		GameObject child = null;
		DoForHierarchy(root, false, (GameObject node) =>
		{
			if (checkNode(node))
			{
				child = node;
				return false;
			}
			return true;
		});
		return child;
	}

	public static GameObject FindCustomChild(this MonoBehaviour behaviour, Func<GameObject, bool> checkNode)
	{
		return behaviour.gameObject.FindCustomChild(checkNode);
	}

	public static List<GameObject> FindCustomChildren(this GameObject root, Func<GameObject, bool> checkNode)
	{
		List<GameObject> children = new List<GameObject>();
		DoForHierarchy(root, false, (GameObject node) =>
		{
			if (checkNode(node))
			{
				children.Add(node);
				return false;
			}
			return true;
		});
		return children;
	}

	public static List<GameObject> FindCustomChildren(this MonoBehaviour behaviour, Func<GameObject, bool> checkNode)
	{
		return behaviour.gameObject.FindCustomChildren(checkNode);
	}

	public static GameObject FindChildWithComponent<T>(this GameObject root) where T : Component
	{
		return root.FindCustomChild((GameObject node) => node.GetComponent<T>() != null);
	}

	public static GameObject FindChildWithComponent<T>(this MonoBehaviour behaviour) where T : Component
	{
		return behaviour.gameObject.FindChildWithComponent<T>();
	}

	public static GameObject FindChildInHierarchy(this GameObject obj, string name)
	{
		return obj.FindCustomChild((GameObject node) => node.name == name);
	}

	public static GameObject FindChildInHierarchy(this MonoBehaviour behaviour, string name)
	{
		return behaviour.gameObject.FindChildInHierarchy(name);
	}

	public static GameObject FindOrCreate(string name)
	{
		return FindOrCreate(null, name);
	}

	public static GameObject FindOrCreate(GameObject parent, string name)
	{
		GameObject gameObject = !parent ? Find(name) : parent.FindChild(name.TrimStart('/'));
		if (!gameObject)
		{
			gameObject = new GameObject(name.TrimStart('/'));
		}
		if (parent != null)
		{
			gameObject.transform.parent = parent.transform;
		}
		return gameObject;
	}

	public static GameObject RemoveClonePostfix(this GameObject obj)
	{
		string str = obj.name;
		bool flag = TryRemovePostfix(ref str, "(Clone)");
		obj.name = str;
		return obj;
	}

	public static void SetGameObjectVisible(GameObject obj, bool visible)
	{
		SetGameObjectVisible(obj, visible, new HashSet<string>());
	}

	public static void SetGameObjectVisible(GameObject obj, bool visible, HashSet<string> ignores)
	{
		DoForHierarchy(obj, (GameObject node) =>
		{
			if (ignores.Contains(node.name))
			{
				return false;
			}
			if (node.GetComponent<Renderer>() != null)
			{
				node.GetComponent<Renderer>().enabled = visible;
			}
			return true;
		});
	}

	public static void MarkAllChildrenAsClone(GameObject go)
	{
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (!transform.gameObject.name.EndsWith("(Clone)"))
			{
				transform.gameObject.name += "(Clone)";
			}
		}
	}

	public static bool GetPathForObjectInHierarchy(GameObject childGO, GameObject baseGO, ref string path)
	{
		bool flag = false;
		path = string.Empty;
		Transform transform = childGO.transform;
		while (transform != null)
		{
			if (transform == baseGO.transform)
			{
				flag = true;
				break;
			}
			if (path.Length == 0)
			{
				path = transform.name;
			}
			else
			{
				path = transform.name + "/" + path;
			}
			transform = transform.parent;
		}
		if (!flag)
		{
			path = string.Empty;
		}
		return flag;
	}

	public static T FindObjectOfType<T>() where T : Component
	{
		return UnityEngine.Object.FindObjectOfType(typeof(T)) as T;
	}

	public static T[] FindObjectsOfType<T>() where T : Component
	{
		return UnityEngine.Object.FindObjectsOfType(typeof(T)) as T[];
	}

	public static bool DoRarelyReal(object obj, string name, float minDelay)
	{
		return DoRarely(obj, name, minDelay, true);
	}

	public static bool DoRarely(object obj, string name, float minDelay)
	{
		return DoRarely(obj, name, minDelay, false);
	}

	public static bool DoRarely(object obj, string name, float minDelay, bool useRealTime)
	{
		Dictionary<string, float> value;
		if (!rarelyMap.TryGetValue(obj, out value))
		{
			value = new Dictionary<string, float>();
			rarelyMap.Add(obj, value);
		}
		float num = useRealTime ? TimeControl.RealTime : Time.time;
		float value2;
		if (!value.TryGetValue(name, out value2))
		{
			value.Add(name, num);
			return true;
		}
		float num2 = 1f + 0.1f * ((float)(((obj != null) ? obj.GetHashCode() : 0) % 997) / 997f);
		if (num - value2 >= minDelay * num2)
		{
			value[name] = num;
			return true;
		}
		return false;
	}

	public static void DoRarelyReset(object obj, string name)
	{
		if (rarelyMap.ContainsKey(obj) && rarelyMap[obj].ContainsKey(name))
		{
			rarelyMap[obj].Remove(name);
		}
	}

	public static string SafeFormat(string format, params object[] args)
	{
		string result;
		try
		{
			result = string.Format(format, args);
		}
		catch
		{
			result = "INVALID FORMAT";
		}
		return result;
	}

	public static bool TryRemovePrefix(ref string str, string prefix)
	{
		if (!str.StartsWith(prefix))
		{
			return false;
		}
		str = str.Substring(prefix.Length);
		return true;
	}

	public static void RemovePrefix(ref string str, string prefix)
	{
		bool flag = TryRemovePrefix(ref str, prefix);
	}

	public static bool TryRemovePostfix(ref string str, string postfix)
	{
		if (!str.EndsWith(postfix))
		{
			return false;
		}
		str = str.Substring(0, str.Length - postfix.Length);
		return true;
	}

	public static void RemovePostfix(ref string str, string postfix)
	{
		bool flag = TryRemovePostfix(ref str, postfix);
	}

	public static int CompareNatural(string strA, string strB)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num < strA.Length && num2 < strB.Length)
		{
			bool flag = IsDigit(strA[num]);
			bool flag2 = IsDigit(strB[num2]);
			if (flag != flag2)
			{
				if (num == 0 && num2 == 0)
				{
					return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
				}
				return flag ? -1 : 1;
			}
			if (!flag && !flag2)
			{
				int i = num + 1;
				int j = num2 + 1;
				for (; i < strA.Length && !IsDigit(strA[i]); i++)
				{
				}
				for (; j < strB.Length && !IsDigit(strB[j]); j++)
				{
				}
				int num4 = string.Compare(strA.Substring(num, i - num), strB.Substring(num2, j - num2), StringComparison.OrdinalIgnoreCase);
				if (num4 != 0)
				{
					return num4;
				}
				num = i;
				num2 = j;
				continue;
			}
			bool flag3 = false;
			bool flag4 = false;
			do
			{
				if (strA[num] != '0')
				{
					flag3 = true;
				}
				else
				{
					num++;
				}
			}
			while (!flag3 && num < strA.Length && IsDigit(strA[num]));
			do
			{
				if (strB[num2] != '0')
				{
					flag4 = true;
				}
				else
				{
					num2++;
				}
			}
			while (!flag4 && num2 < strB.Length && IsDigit(strB[num2]));
			if (flag3 != flag4)
			{
				return flag3 ? 1 : (-1);
			}
			if (flag3 && flag4)
			{
				int num5 = 0;
				while ((flag = num < strA.Length && IsDigit(strA[num])) & (flag2 = num2 < strB.Length && IsDigit(strB[num2])))
				{
					if (strA[num] != strB[num2] && num5 == 0)
					{
						num5 = strA[num] < strB[num2] ? -1 : 1;
					}
					num++;
					num2++;
				}
				if (flag != flag2)
				{
					return flag ? 1 : (-1);
				}
				if (num5 != 0)
				{
					return num5;
				}
			}
			if (num != num2 && num3 == 0)
			{
				num3 = num > num2 ? -1 : 1;
			}
		}
		if (num < strA.Length || num2 < strB.Length)
		{
			return (num < strA.Length) ? 1 : (-1);
		}
		if (num3 != 0)
		{
			return num3;
		}
		return 0;
	}

	public static bool IsDigit(char c)
	{
		return c >= '0' && c <= '9';
	}

	public static string[] Split(this string str, params string[] dividers)
	{
		return str.Split(dividers, StringSplitOptions.None);
	}

	public static string Implode<T>(this IEnumerable<T> array, string divider, Func<T, string> elemToString)
	{
		string text = string.Empty;
		bool flag = true;
		foreach (T item in array)
		{
			if (!flag)
			{
				text += divider;
			}
			else
			{
				flag = false;
			}
			text += elemToString(item);
		}
		return text;
	}

	public static List<string> MakeChoice(string input)
	{
		int num = 1;
		string str = input;
		string[] array = input.Split(":");
		if (array.Length >= 2)
		{
			num = int.Parse(array[0]);
			str = array[1];
		}
		else if (input.All((char c) => IsDigit(c)))
		{
			return (from n in Enumerable.Range(0, (input != string.Empty) ? int.Parse(input) : 0)
				select "any").ToList();
		}
		HashSet<string> hashSet = str.Split("/").ToHashSet();
		List<string> list = new List<string>();
		for (int num2 = 0; num2 < num; num2++)
		{
			string item = RandomFrom((IEnumerable<string>)hashSet);
			if (num - num2 <= hashSet.Count)
			{
				hashSet.Remove(item);
			}
			list.Add(item);
		}
		return list;
	}

	public static void ClearCaches()
	{
		ResetFindChild();
		ResetFind();
		rarelyMap.Clear();
	}

	public static T GetFirst<T>(IEnumerable<T> container)
	{
		IEnumerator<T> enumerator = container.GetEnumerator();
		bool flag = enumerator.MoveNext();
		return enumerator.Current;
	}

	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
	{
		return new HashSet<T>(source);
	}

	public static Vector3 DistanceToSegment(Vector3 point, Vector3 pA, Vector3 pB, out bool projectionInSegment)
	{
		Vector3 vector = pB - pA;
		Vector3 rhs = point - pA;
		float num = Vector3.Dot(vector, rhs);
		float num2 = Vector3.Dot(vector, vector);
		projectionInSegment = false;
		if (num <= 0f)
		{
			return point - pA;
		}
		if (num2 <= num)
		{
			return point - pB;
		}
		projectionInSegment = true;
		float num3 = num / num2;
		Vector3 vector2 = pA + vector * num3;
		return point - vector2;
	}

	public static void Invert(ref bool b)
	{
		b = !b;
	}

	public static bool SamplePosition(ref Vector3 position)
	{
		UnityEngine.AI.NavMeshHit hit;
		UnityEngine.AI.NavMesh.SamplePosition(position, out hit, 5f, -1);
		if (!hit.hit)
		{
			return false;
		}
		position = hit.position;
		return true;
	}

	public static bool FindPath(Vector3 from, Vector3 to, out UnityEngine.AI.NavMeshPath path)
	{
		return FindPath(from, to, out path, false, true);
	}

	public static bool FindPath(Vector3 from, Vector3 to, out UnityEngine.AI.NavMeshPath path, bool sampleFrom, bool sampleTo)
	{
		path = new UnityEngine.AI.NavMeshPath();
		while (true)
		{
			bool foundPath = UnityEngine.AI.NavMesh.CalculatePath(from, to, -1, path);
			if (foundPath)
			{
				return true;
			}
			if (sampleTo)
			{
				sampleTo = false;
				if (!SamplePosition(ref to))
				{
					return false;
				}
			}
			else if (sampleFrom)
			{
				sampleFrom = false;
				if (!SamplePosition(ref from))
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}

	public static float CalcNavDistance(Vector3 from, Vector3 to)
	{
		UnityEngine.AI.NavMeshPath path;
		return !(!FindPath(from, to, out path)) ? PathHelper.GetPathLength(path) : (-1f);
	}

	public static void RotateToCamera(Transform transform)
	{
		transform.forward = Scene.MainCamera.transform.forward;
	}

	public static string BuildTag()
	{
		return BuildInfo.buildTag;
	}

	public static string IsoTime()
	{
		return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
	}

	public static Color ColorFromHex(string hex)
	{
		hex = hex.TrimStart('#');
		return new Color((float)Convert.ToInt32(hex.Substring(0, 2), 16) / 255f, (float)Convert.ToInt32(hex.Substring(2, 2), 16) / 255f, (float)Convert.ToInt32(hex.Substring(4, 2), 16) / 255f, 1f);
	}
}

[Serializable]
public class FieldModification
{
	public string field;

	public string component;

	public string value;
}

public static class PathHelper
{
	public static Vector3 FindNearestPointOnPath(List<Vector3> path, Vector3 point, out int index)
	{
		Vector3 result = Vector3.zero;
		float num = float.MaxValue;
		index = 0;
		for (int i = 0; i < path.Count - 1; i++)
		{
			bool projectionInSegment;
			Vector3 vector = Util.DistanceToSegment(point, path[i], path[i + 1], out projectionInSegment);
			if (vector.sqrMagnitude < num)
			{
				result = point - vector;
				index = i;
				num = vector.sqrMagnitude;
			}
		}
		return result;
	}

	public static float GetPathLength(UnityEngine.AI.NavMeshPath path)
	{
		return GetPathLength(path, 0);
	}

	public static float GetPathLength(UnityEngine.AI.NavMeshPath path, int startIndex)
	{
		float num = 0f;
		for (int i = startIndex; i < path.corners.Length - 1; i++)
		{
			num += (path.corners[i + 1] - path.corners[i]).magnitude;
		}
		return num;
	}
}

namespace System
{
	public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}
