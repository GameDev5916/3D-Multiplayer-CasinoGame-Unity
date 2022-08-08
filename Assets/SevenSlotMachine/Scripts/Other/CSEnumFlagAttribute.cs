using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSEnumFlagAttribute : PropertyAttribute {
    public string enumName;

    public CSEnumFlagAttribute() { }

    public CSEnumFlagAttribute(string name)
    {
        enumName = name;
    }
}
