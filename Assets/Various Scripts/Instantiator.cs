using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Instantiator : ScriptableObject
{

    [SerializeField] private List<GameObject> _prefabsToInstantiate;

    private List<GameObject> _instantiatedGameObjects = new List<GameObject>();

    private void Awake()
    {
        Debug.Log("Awake");
    }

    private void OnEnable()
    {
        Application.quitting += Application_quitting;
        Debug.Log("OnEnable");

        foreach (var prefab in _prefabsToInstantiate)
        {
            if (prefab != null)
            {
                //GameObject newInstance = Instantiate(prefab);
                //_instantiatedGameObjects.Add(newInstance);
            }
        }
    }

    private void Application_quitting()
    {
        Debug.Log("hej");
    }

    private void OnDisable()
    {
        //foreach(var instance in _instantiatedGameObjects)
        //{
        //    DestroyImmediate(instance);
        //}
        _instantiatedGameObjects.Clear();
    }
}
