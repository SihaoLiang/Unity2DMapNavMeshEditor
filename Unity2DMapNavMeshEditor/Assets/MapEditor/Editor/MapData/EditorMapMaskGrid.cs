using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EditorMapMaskGrid
{
    public int x;
    public int y;
    public int mask;


    public override bool Equals(object obj)
    {
        if (!(obj is EditorMapMaskGrid))
			  return false;

        EditorMapMaskGrid p = (EditorMapMaskGrid)obj;
        return this.x == p.x && this.y == p.y;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
