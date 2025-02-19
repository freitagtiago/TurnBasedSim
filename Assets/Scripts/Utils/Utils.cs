using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Color HexToColor(string hex)
    {
        // Remove o caractere '#' se existir
        hex = hex.Replace("#", "");

        // Converte a string hexadecimal para valores inteiros
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        // Converte os valores para o range 0-1 e retorna a cor
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static string GetHexCodeForElement(ElementType element)
    {
        string color = "";
        switch (element)
        {
            case ElementType.Icy:
                color = "#8AD2E3";
                break;
            case ElementType.Burning:
                color = "#E28A8D";
                break;
            case ElementType.Organic:
                color = "#E2A78A";
                break;
            case ElementType.Tempest:
                color = "#91E28A";
                break;
            case ElementType.Luminous:
                color = "#F3F340";
                break;
            case ElementType.Savage:
                color = "#EBB1F5";
                break;
            case ElementType.Darkness:
                color = "#C6B2F6";
                break;
            case ElementType.Spiritual:
                color = "#FF64CD";
                break;
            default:
                color = "#FFFFFF";
                break;
        }
        return color;
    }
}
