using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class UIPackable : MonoBehaviour, ISpriteAggregator, ISpritePackable
{
	public float Width;

	public float Height;

	public UIAlign.Horizontal Horizontal = UIAlign.Horizontal.Center;

	public UIAlign.Vertical Vertical = UIAlign.Vertical.Center;

	public Color SpriteColor = Color.white;

	protected Mesh _mesh;

	protected Transform _transform;

	protected int _numVertices = 4;

	protected int _numTriangles = 2;

	private Texture2D[] AggregateTextures;

	private CSpriteFrame[] AggregateFrames;

	public TextureAnim[] TextureAnimations = new TextureAnim[0];

	protected virtual void Awake()
	{
		Init();
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			Init();
		}
	}

	protected virtual void Init()
	{
		_transform = GetComponent<Transform>();
		_mesh = CreateMesh();
		MeshFilter component = GetComponent<MeshFilter>();
		component.mesh = _mesh;
	}

	protected virtual Mesh CreateMesh()
	{
		int[] triangles = new int[_numTriangles * 3];
		Vector2[] array = new Vector2[_numVertices];
		Vector3[] vertices = new Vector3[_numVertices];
		Color[] colors = new Color[_numVertices];
		CalculateVertices(vertices);
		CalculateTriangles(triangles);
		CalculateUVs(vertices, array);
		UpdateAlign(vertices);
		UpdateColors(colors, SpriteColor);
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = array;
		mesh.triangles = triangles;
		mesh.colors = colors;
		UpdateBounds(mesh);
		return mesh;
	}

	protected virtual void CalculateVertices(Vector3[] vertices)
	{
	}

	protected virtual void CalculateTriangles(int[] triangles)
	{
	}

	protected virtual void CalculateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		if (!(Width * Height != 0f))
		{
			return;
		}
		Rect defaultUVs = DefaultUVs;
		float width = defaultUVs.width;
		float height = defaultUVs.height;
		for (int i = 0; i < _numVertices; i++)
		{
			uvs[i].x = vertices[i].x / Width * width + defaultUVs.x;
			uvs[i].y = vertices[i].y / Height * height + defaultUVs.y;
		}
	}

	protected virtual void UpdateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		CalculateUVs(vertices, uvs);
	}

	private void GetMinMax(Vector3[] vertices, out Vector2 min, out Vector2 max)
	{
		min = new Vector2(float.MaxValue, float.MaxValue);
		max = new Vector2(float.MinValue, float.MinValue);
		foreach (Vector3 vector in vertices)
		{
			if (vector.x < min.x)
			{
				min.x = vector.x;
			}
			if (vector.x > max.x)
			{
				max.x = vector.x;
			}
			if (vector.y < min.y)
			{
				min.y = vector.y;
			}
			if (vector.y > max.y)
			{
				max.y = vector.y;
			}
		}
	}

	protected virtual void UpdateBounds(Mesh mesh)
	{
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}

	protected virtual void UpdateAlign(Vector3[] vertices)
	{
		Vector2 min;
		Vector2 max;
		GetMinMax(vertices, out min, out max);
		Vector2 zero = Vector2.zero;
		switch (Horizontal)
		{
		case UIAlign.Horizontal.Left:
			zero.x = -min.x;
			break;
		case UIAlign.Horizontal.Right:
			zero.x = -max.x;
			break;
		case UIAlign.Horizontal.Center:
			zero.x = (-(min.x + max.x)) / 2f;
			break;
		}
		switch (Vertical)
		{
		case UIAlign.Vertical.Top:
			zero.y = -max.y;
			break;
		case UIAlign.Vertical.Bottom:
			zero.y = -min.y;
			break;
		case UIAlign.Vertical.Center:
			zero.y = (-(min.y + max.y)) / 2f;
			break;
		}
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i].x += zero.x;
			vertices[i].y += zero.y;
		}
	}

	private void UpdateColors(Color[] colors, Color color)
	{
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = color;
		}
		SpriteColor = color;
	}

	private void SetColor(Color color)
	{
		Color[] colors = _mesh.colors;
		UpdateColors(colors, color);
		_mesh.colors = colors;
	}

	public void Aggregate(PathFromGUIDDelegate guid2Path, LoadAssetDelegate load, GUIDFromPathDelegate path2Guid)
	{
		List<Texture2D> list = new List<Texture2D>();
		List<CSpriteFrame> list2 = new List<CSpriteFrame>();
		foreach (TextureAnim textureAnim in States)
		{
			textureAnim.Allocate();
			if (textureAnim.frameGUIDs.Length >= textureAnim.framePaths.Length)
			{
				for (int j = 0; j < textureAnim.frameGUIDs.Length; j++)
				{
					string path = guid2Path(textureAnim.frameGUIDs[j]);
					Object obj = load(path, typeof(Texture2D));
					list.Add(obj as Texture2D);
					list2.Add(textureAnim.spriteFrames[j]);
				}
				textureAnim.framePaths = new string[0];
				continue;
			}
			textureAnim.frameGUIDs = new string[textureAnim.framePaths.Length];
			textureAnim.spriteFrames = new CSpriteFrame[textureAnim.framePaths.Length];
			for (int k = 0; k < textureAnim.spriteFrames.Length; k++)
			{
				textureAnim.spriteFrames[k] = new CSpriteFrame();
			}
			for (int l = 0; l < textureAnim.framePaths.Length; l++)
			{
				textureAnim.frameGUIDs[l] = path2Guid(textureAnim.framePaths[l]);
				Object obj2 = load(textureAnim.framePaths[l], typeof(Texture2D));
				list.Add(obj2 as Texture2D);
				list2.Add(textureAnim.spriteFrames[l]);
			}
		}
		AggregateTextures = list.ToArray();
		AggregateFrames = list2.ToArray();
	}

	public Texture2D[] SourceTextures
	{
		get
		{
			return AggregateTextures;
		}
	}

	public CSpriteFrame[] SpriteFrames
	{
		get
		{
			return AggregateFrames;
		}
	}

	public CSpriteFrame DefaultFrame
	{
		get
		{
			if (States.Length != 0 && States[0].spriteFrames.Length != 0)
			{
				return States[0].spriteFrames[0];
			}
			return null;
		}
	}

	public Rect DefaultUVs
	{
		get
		{
			Rect result = new Rect(0f, 0f, 1f, 1f);
			CSpriteFrame defaultFrame = DefaultFrame;
			if (defaultFrame != null)
			{
				result = defaultFrame.uvs;
			}
			return result;
		}
	}

	public Material GetPackedMaterial(out string errString)
	{
		errString = "Sprite \"" + name + "\" has not been assigned a material!";
		Renderer component = GetComponent<Renderer>();
		return component.sharedMaterial;
	}

	public void SetUVs(Rect uvs)
	{
		Init();
	}

	public TextureAnim[] States
	{
		get
		{
			return TextureAnimations;
		}
		set
		{
			TextureAnimations = value;
		}
	}

	public Color Color
	{
		get
		{
			return SpriteColor;
		}
		set
		{
			SetColor(value);
		}
	}

	public SpriteRoot.ANCHOR_METHOD Anchor
	{
		get
		{
			return SpriteRoot.ANCHOR_METHOD.MIDDLE_CENTER;
		}
	}

	public bool SupportsArbitraryAnimations
	{
		get
		{
			return true;
		}
	}

	public bool DoNotTrimImages
	{
		get
		{
			return true;
		}
	}
}
