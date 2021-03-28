using System;
using UnityEngine;

public class HairTypeArrayAttribute : PropertyAttribute
{
    public readonly string[] names = new string[Enum.GetNames(typeof(LonghairSupporter.HairType)).Length];
    public HairTypeArrayAttribute()
    {
        int i = 0;
        foreach (string name in Enum.GetNames(typeof(LonghairSupporter.HairType)))
        {
            names[i] = name;
            i++;
        }
    }
}