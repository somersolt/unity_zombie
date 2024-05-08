using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ColorSerialization
{
    public static byte[] SerializeColor(object target)
    {
        Color color = (Color)target;
        byte[] result = new byte[sizeof(float) * 4];

        System.Array.Copy(BitConverter.GetBytes(color.r), 0, result, 0, 4);
        System.Array.Copy(BitConverter.GetBytes(color.g), 0, result, 4, 4);
        System.Array.Copy(BitConverter.GetBytes(color.b), 0, result, 8, 4);
        System.Array.Copy(BitConverter.GetBytes(color.a), 0, result, 12, 4);
        return result;
    }

    public static object DeserializeColor(byte[] bytes)
    {
        Color color;
        color.r = BitConverter.ToSingle(bytes, 0);
        color.g = BitConverter.ToSingle(bytes, 4);
        color.b = BitConverter.ToSingle(bytes, 8);
        color.a = BitConverter.ToSingle(bytes, 12);
        return color;

    }
}
