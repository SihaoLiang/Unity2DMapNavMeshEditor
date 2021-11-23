using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BrushType
{
    NONE,
    MASK,
    POSITION
}
public class MapEditorBrush
{
    public enum BrushShapeType
    {
        DEFAULT = 0,
        CROSS,
        DIAMOND,
        SQUARE,
        COUNT
    }

    public static Vector2Int[][] BrushShape = new Vector2Int[(int)BrushShapeType.COUNT][]
    {
        new Vector2Int[1]
        {
            new Vector2Int(0, 0),
        },
        new Vector2Int[5]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        },
        new Vector2Int[13]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(0, 2),
            new Vector2Int(2, 0),
            new Vector2Int(0, -2),
            new Vector2Int(-2, 0),
        },
        new Vector2Int[9]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        },
    };
    public BrushType brushType = BrushType.MASK;
    public int id;
    public Color color;
    public EditorMapPosGrid grid;
    public string groupKey;

}
