using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBT_Unit : MonoBehaviour
{
    public static List<RBT_Unit> ActiveUnits { get; private set; } = new List<RBT_Unit>();

    private void OnEnable()
    {
        ActiveUnits.Add(this);
        Debug.Log($"{ActiveUnits.Count}");
    }

    private void OnDisable()
    {
        ActiveUnits.Remove(this);
        Debug.Log($"{ActiveUnits.Count}");
    }
}
