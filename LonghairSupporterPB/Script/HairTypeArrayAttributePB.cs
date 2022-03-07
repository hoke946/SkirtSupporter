using System;
using UnityEngine;

public class HairTypeArrayPBAttribute : PropertyAttribute
{
    public readonly string[] names = new string[Enum.GetNames(typeof(LonghairSupporterPB.HairType)).Length];
    public HairTypeArrayPBAttribute()
    {
        int i = 0;
        foreach (string name in Enum.GetNames(typeof(LonghairSupporterPB.HairType)))
        {
            names[i] = name;
            i++;
        }
    }
}