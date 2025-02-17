using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TypeMatchup
{
    public ElementType _elementalType;
    public List<ElementType> _strongAgainst = new List<ElementType>();
    public List<ElementType> _weakAgainst = new List<ElementType>();
    public List<ElementType> _resistences = new List<ElementType>();
}
