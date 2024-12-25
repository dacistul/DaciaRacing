using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{

    [Header("Car Front (Transform)")]// Assign a Gameobject representing the front of the car
    public Transform carFront;

    [Header("General Parameters")]// Look at the documentation for a detailed explanation
    public List<string> NavMeshLayers;

    [Header("Debug")]
    public bool ShowGizmos;
    public bool Debugger;

    [Header("Destination Parameters")]// Look at the documentation for a detailed explanation
    public Transform CustomDestination;

    [HideInInspector] public bool move;// Look at the documentation for a detailed explanation

    private Vector3 PostionToFollow = Vector3.zero;
    private float AIFOV = 60;
    private bool allowMovement;
    private int NavMeshLayerBite;
    public LinkedList<Vector3> waypoints = new LinkedList<Vector3>();

    private CarController car;

    private Vector3 endDirection() {

        var last = waypoints.Last;
        var secondLast = last.Previous;

        return (secondLast.Value-last.Value).normalized;
    }
    private bool peek(out Vector3 vout)
    {
        if(waypoints.First == null)
        {
            vout = Vector3.zero;
            return false;
        }
        vout = waypoints.First.Value;
        return true;
    }
    private bool reversePeek(out Vector3 vout)
    {
        if(waypoints.Last == null)
        {
            vout = Vector3.zero;
            return false;
        }
        vout = waypoints.Last.Value;
        return true;
    }
    private Vector3 pop() {
        var temp = waypoints.First.Value;
        waypoints.RemoveFirst();
        return temp;
    }

    private void addFront(Vector3 v) {
        waypoints.AddFirst(v);
    }

    private void addBack(Vector3 v) {
        waypoints.AddLast(v);
    }
    private void addBack(System.Collections.Generic.IEnumerable<Vector3> arr) {
        foreach(var v in arr)
            waypoints.AddLast(v);
    }

    void Awake()
    {
        allowMovement = true;
        move = true;
    }

    void Start()
    {
        //waypoints = new LinkedList<Vector3>();
        car = GetComponent<CarController>();
        CalculateNavMashLayerBite();
    }

    void FixedUpdate()
    {
        PathProgress();
    }

    private void CalculateNavMashLayerBite()
    {
        NavMeshLayerBite = NavMesh.AllAreas;
    }

    void ApplySteering() // Applies steering to the Current waypoint
    {
        // TODO room for code that increases the speed in a straight line
        Vector3 relativeVector = transform.InverseTransformPoint(PostionToFollow).normalized;

        car.steer(relativeVector.x);
    }

    private void PathProgress() //Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
    {
        wayPointManager();
        ApplySteering();
        Movement();
        ListOptimizer();

        void wayPointManager()
        {
            allowMovement = peek(out PostionToFollow);
            if (allowMovement && Vector3.Distance(carFront.position, PostionToFollow) < 2)
                pop();

            if (waypoints.Count < 3)
                CreatePath();
        }

        void CreatePath()
        {
            if (CustomDestination)
            {
               CustomPath(CustomDestination);
            }
            else
            {
                debug("No custom destination assigned", false);
                allowMovement = false;
            }

        }

        void ListOptimizer()
        {
            // TODO maybe good idea
        }
    }

    public void CustomPath(Transform destination) //Creates a path to the Custom destination
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 sourcePostion = carFront.position;
        Vector3 direction = carFront.forward;

        if(waypoints.Count >= 1)
            reversePeek(out sourcePostion);
        if(waypoints.Count>=2)
            direction = endDirection();


        if (NavMesh.SamplePosition(destination.position, out NavMeshHit hit, 150, NavMeshLayerBite) &&
            NavMesh.CalculatePath(sourcePostion, hit.position, NavMeshLayerBite, path))
        {
            if (path.corners.Length > 1&& CheckForAngle(path.corners[1], sourcePostion, direction))
            {
                addBack(path.corners);
                debug("Custom Path generated successfully", false);
            }
        }
        else
        {
            debug("Failed to generate a Custom path. Invalid Path. Generating a new one", false);
        }
    }

    private bool CheckForAngle(Vector3 pos, Vector3 source, Vector3 direction) //calculates the angle between the car and the waypoint
    {
        Vector3 distance = (pos - source).normalized;
        float CosAngle = Vector3.Dot(distance, direction);
        float Angle = Mathf.Acos(CosAngle) * Mathf.Rad2Deg;

        return Angle < AIFOV;
    }

    void Movement() // moves the car forward and backward depending on the value of MovementTorque
    {
        if (move && allowMovement){
            car.accelerate(0.2f);
            car.brake(0.0f);
        } else {
            car.accelerate(0.0f);
            car.brake(0.25f);
        }
    }

    void debug(string text, bool IsCritical)
    {
        if (Debugger)
            if (IsCritical)
                Debug.LogError(text);
            else
                Debug.Log(text);
    }

    private void OnDrawGizmos() // shows a Gizmos representing the waypoints and AI FOV
    {
        if (ShowGizmos)
        {
            var first = true;
            foreach (var waypoint in waypoints)
            {
                Gizmos.color = first ? Color.blue : Color.red;
                Gizmos.DrawWireSphere(waypoint, 2f);
                first = false;
            }
            Gizmos.color = Color.white;
            foreach(var fov in new float[]{-AIFOV, AIFOV})
            {
                Vector3 rayV = Quaternion.AngleAxis(fov, Vector3.up) * transform.forward;
                Gizmos.DrawRay(carFront.position, rayV * 10.0f);
            }
        }
    }
}
