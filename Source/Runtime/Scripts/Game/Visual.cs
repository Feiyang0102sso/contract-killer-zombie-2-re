using System.Collections.Generic;
using UnityEngine;
using System;

public class Visual : MonoBehaviourAndLog
{
	public const int HideOutlineInVisibility = 1;

	public const int OnlyDisableOtherOutlines = 2;

	private Material DepthMask;

	private Material DepthMaskOutline;

	private Dictionary<GameObject, KeyValuePair<bool, Color>> hidableOutline = new Dictionary<GameObject, KeyValuePair<bool, Color>>();

	public void Reset()
	{
		hidableOutline.Clear();
	}

	private void Awake()
	{
		LogTo("Other.Visual");
		DepthMask = Resources.Load("DepthMask", typeof(Material)) as Material;
		DepthMaskOutline = Resources.Load("DepthMaskOutline", typeof(Material)) as Material;
	}

	private void Update()
	{
		if (Util.DoRarelyReal(this, "update_outline", 0.5f))
		{
			Dictionary<GameObject, KeyValuePair<bool, Color>> dictionary = new Dictionary<GameObject, KeyValuePair<bool, Color>>(hidableOutline);
			foreach (KeyValuePair<GameObject, KeyValuePair<bool, Color>> item in dictionary)
			{
				GameObject key = item.Key;
				bool key2 = item.Value.Key;
				Color value = item.Value.Value;
				if (!key)
				{
					hidableOutline.Remove(key);
					continue;
				}
				Vector3 position = Scene.MainCamera.transform.position;
				Character component = key.GetComponent<Character>();
				Vector3 vector = component ? component.Center() : key.transform.position;
				RaycastHit hitInfo;
				bool flag = Physics.Raycast(position, vector - position, out hitInfo, Vector3.Distance(vector, position), Const.LayerMaskObstacles);
				if (key2 != flag)
				{
					if (!key2 && flag)
					{
						log.Debug(key, "outline shown in not-visibility '{0}'", key.name);
						ShowOutlineInternal(key, value, 0);
					}
					else if (key2 && !flag)
					{
						log.Debug(key, "outline hidden in visibility for '{0}'", key.name);
						HideOutlineInternal(key);
					}
					hidableOutline[key] = new KeyValuePair<bool, Color>(!key2, value);
				}
			}
		}
	}

	public void ShowOutline(GameObject obj, Color color, int options)
	{
		log.Debug(obj, "showing outline for '{0}'", obj.name);
		if ((options & 1) != 0)
		{
			hidableOutline[obj] = new KeyValuePair<bool, Color>(true, color);
		}
		else
		{
			ShowOutlineInternal(obj, color, options);
		}
	}

	private void ShowOutlineInternal(GameObject obj, Color color, int options)
	{
		SkinnedMeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		foreach (SkinnedMeshRenderer mesh in componentsInChildren)
		{
			ShowOutline(mesh, color, options);
		}
		MeshRenderer[] componentsInChildren2 = obj.GetComponentsInChildren<MeshRenderer>(true);
		foreach (MeshRenderer meshRenderer in componentsInChildren2)
		{
			if (meshRenderer.gameObject.layer != Const.LayerTransparentFX && meshRenderer.gameObject.layer != Const.LayerGui3d)
			{
				ShowOutline(meshRenderer, color, options);
			}
		}
	}

	public void HideOutline(GameObject obj)
	{
		log.Debug(obj, "hiding outline for '{0}'", obj.name);
		if (hidableOutline.ContainsKey(obj))
		{
			hidableOutline.Remove(obj);
		}
		HideOutlineInternal(obj);
	}

	private void HideOutlineInternal(GameObject obj)
	{
		SkinnedMeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		foreach (SkinnedMeshRenderer mesh in componentsInChildren)
		{
			HideOutline(mesh);
		}
		MeshRenderer[] componentsInChildren2 = obj.GetComponentsInChildren<MeshRenderer>(true);
		foreach (MeshRenderer meshRenderer in componentsInChildren2)
		{
			if (meshRenderer.gameObject.layer != Const.LayerTransparentFX && meshRenderer.gameObject.layer != Const.LayerGui3d)
			{
				HideOutline(meshRenderer);
			}
		}
	}

	private void ShowOutline(Renderer mesh, Color color, int options)
	{
		if (mesh.materials.Length == 1)
		{
			Material material = mesh.materials[0];
			bool flag = (options & 2) == 0;
			Material[] array = new Material[flag ? 3 : 2];
			array[0] = material;
			array[1] = DepthMask;
			if (flag)
			{
				array[2] = new Material(DepthMaskOutline);
				array[2].SetColor("_SilhouetteColor", color);
			}
			mesh.materials = array;
		}
	}

	private void HideOutline(Renderer mesh)
	{
		bool flag = false;
		foreach (Material material in mesh.materials)
		{
			if (material.shader == DepthMask.shader)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Material[] materials = new Material[1];
			materials[0] = mesh.materials[0];
			mesh.materials = materials;
		}
	}
}

public class LightProbeSampler
{
	public static Vector4 SHAr;

	public static Vector4 SHAg;

	public static Vector4 SHAb;

	public static Vector4 SHBr;

	public static Vector4 SHBg;

	public static Vector4 SHBb;

	public static Vector4 SHC;

	private static float[] _probe = new float[27];

	private static float[,] _sh = new float[9, 3];

	private static Vector4[] _vCoeff = new Vector4[3];

	private static Vector3 _x1 = Vector3.zero;

	private static Vector3 _x2 = Vector3.zero;

	private static Vector3 _x3 = Vector3.zero;

	private static Vector4 _vB = Vector4.zero;

	private static float _vC = 0f;

	private static Vector3 _sample = Vector4.zero;

	private static Color _result = Color.black;

	private static float SqrtPI = Mathf.Sqrt((float)Math.PI);

	private static float fC0 = 1f / (2f * SqrtPI);

	private static float fC1 = Mathf.Sqrt(3f) / (3f * SqrtPI);

	private static float fC2 = Mathf.Sqrt(15f) / (8f * SqrtPI);

	private static float fC3 = Mathf.Sqrt(5f) / (16f * SqrtPI);

	private static float fC4 = 0.5f * fC2;

	public static void InterpolateSH(Vector3 position, Renderer renderer)
	{
		NewUnitySceneLighting.GetInterpolatedLightProbe(position, renderer, _probe);
		int num = 0;
		for (int i = 0; i < 9; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				_sh[i, j] = _probe[num];
				num++;
			}
		}
		for (int k = 0; k < 3; k++)
		{
			_vCoeff[k].x = (-fC1) * _sh[3, k];
			_vCoeff[k].y = (-fC1) * _sh[1, k];
			_vCoeff[k].z = fC1 * _sh[2, k];
			_vCoeff[k].w = fC0 * _sh[0, k] - fC3 * _sh[6, k];
		}
		SHAr = _vCoeff[0];
		SHAg = _vCoeff[1];
		SHAb = _vCoeff[2];
		for (int l = 0; l < 3; l++)
		{
			_vCoeff[l].x = fC2 * _sh[4, l];
			_vCoeff[l].y = (-fC2) * _sh[5, l];
			_vCoeff[l].z = 3f * fC3 * _sh[6, l];
			_vCoeff[l].w = (-fC2) * _sh[7, l];
		}
		SHBr = _vCoeff[0];
		SHBg = _vCoeff[1];
		SHBb = _vCoeff[2];
		_vCoeff[0].x = fC4 * _sh[8, 0];
		_vCoeff[0].y = fC4 * _sh[8, 1];
		_vCoeff[0].z = fC4 * _sh[8, 2];
		_vCoeff[0].w = 1f;
		SHC = _vCoeff[0];
	}

	public static Color ShadeSH(Vector3 normal)
	{
		_x1 = Vector3.zero;
		_x2 = Vector3.zero;
		_x3 = Vector3.zero;
		_x1.x = Vector3.Dot(SHAr, normal);
		_x1.y = Vector3.Dot(SHAg, normal);
		_x1.z = Vector3.Dot(SHAb, normal);
		_vB.x = normal.x * normal.y;
		_vB.y = normal.y * normal.z;
		_vB.z = normal.z * normal.z;
		_vB.w = normal.z * normal.x;
		_x2.x = Vector3.Dot(SHBr, _vB);
		_x2.y = Vector3.Dot(SHBg, _vB);
		_x2.z = Vector3.Dot(SHBb, _vB);
		_vC = normal.x * normal.x - normal.y * normal.y;
		_x3 = SHC * _vC;
		_sample = _x1 + _x2 + _x3;
		_result.r = Mathf.Abs(_sample.x);
		_result.g = Mathf.Abs(_sample.y);
		_result.b = Mathf.Abs(_sample.z);
		_result.a = 1f;
		return _result;
	}
}
