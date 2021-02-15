using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Unit : MonoBehaviour
{
    // Inspector fields
    [SerializeField] private GameObject _selectHighlight;
    [SerializeField] private List<SteeringBehaviour> _steeringsBehaviours;

    // Public events
    public delegate void OnScreenCallback(Unit unit);
    public static event OnScreenCallback OnEnterScreen = delegate { };
    public static event OnScreenCallback OnExitScreen = delegate { };

    // Private variables
    private static List<Unit> _activeUnitsInScene = new List<Unit>();
    private Rigidbody _rigidBody;

    // Public data
    public static List<Unit> ActiveUnitsInScene { get { return _activeUnitsInScene; } private set { _activeUnitsInScene = value; } }
    public Vector3 OnScreenPosition { get { return Camera.main.WorldToScreenPoint(transform.position); } }
    public GameObject SelectHighlight { get { return _selectHighlight; } }
    public Vector3 Velocity { get { return _rigidBody.velocity; } set { _rigidBody.velocity = value; } }
    public Rigidbody RigidBody { get { return _rigidBody; } }
    public Vector3 NetForceOnUnit { get; private set; }

    // Unity event functions
    private void OnEnable()
    {
        if (ActiveUnitsInScene == null)
        {
            ActiveUnitsInScene = new List<Unit>();
        }

        ActiveUnitsInScene.Add(this);
    }

    private void OnDisable()
    {
        ActiveUnitsInScene.Remove(this);
    }

    private void Start()
    {
        InitializeUnit();
    }

    private void Update()
    {
        if (_rigidBody.velocity.magnitude > 0.1f)
        {
            var forward = transform.forward;
            var normalizedVelocity = _rigidBody.velocity.normalized;
            var angleBetweenDirections = Vector3.Angle(forward, normalizedVelocity);

            transform.LookAt(transform.position + _rigidBody.velocity.normalized); 
        }
    }

    private void OnBecameVisible()
    {
        OnEnterScreen.Invoke(this);
    }

    private void OnBecameInvisible()
    {
        OnExitScreen.Invoke(this);
    }

    // Public functions
    public void AddForce(Vector3 force)
    {
        _rigidBody.AddForce(force);
    }

    // Private functions
    private void InitializeUnit()
    {
        _rigidBody = transform.GetComponent<Rigidbody>();

        if (GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }
    }
}
