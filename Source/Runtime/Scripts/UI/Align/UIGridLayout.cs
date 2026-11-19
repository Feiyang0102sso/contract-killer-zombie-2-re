using System.Collections.Generic;
using UnityEngine;

public class UIGridLayout : MonoBehaviour
{
	private class Cell
	{
		public int row;

		public int col;

		public Transform obj;
	}

	public float Width;

	public float Height;

	private int _colCount;

	private int _rowCount;

	private LinkedList<Cell> _cells = new LinkedList<Cell>();

	public void AddItem(GameObject item, int col, int row)
	{
		AddItem(item.transform, col, row);
	}

	public void AddItem(Transform item, int col, int row)
	{
		item.parent = transform;
		Cell cell = new Cell();
		cell.obj = item;
		cell.row = row;
		cell.col = col;
		_rowCount = Mathf.Max(_rowCount, row + 1);
		_colCount = Mathf.Max(_colCount, col + 1);
		_cells.AddLast(cell);
		UpdateLayout();
	}

	public void UpdateLayout()
	{
		foreach (Cell cell in _cells)
		{
			float num = Width / (float)_colCount;
			float num2 = Height / (float)_rowCount;
			float x = num * ((float)cell.col + 0.5f) - Width * 0.5f;
			float num3 = num2 * ((float)cell.row + 0.5f) - Height * 0.5f;
			cell.obj.localPosition = new Vector3(x, -num3, 0f);
		}
	}
}
