using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBTUnit : MonoBehaviour
{
    public static List<RBTUnit> ActiveUnits { get; private set; } = new List<RBTUnit>();

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
