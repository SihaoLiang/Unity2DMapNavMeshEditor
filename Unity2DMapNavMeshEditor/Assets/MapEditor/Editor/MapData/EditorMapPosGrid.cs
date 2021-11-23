using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EditorMapPosGrid
{
    public int x;
    public int y;
    public string key;
    public string group;

    public Color color;
    public string Path;


    public override bool Equals(object obj)
    {
        EditorMapPosGrid other = (EditorMapPosGrid)obj;
        return other.key == key && group == other.group;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
