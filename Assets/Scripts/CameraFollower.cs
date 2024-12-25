using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform targetPosition;
    public float speedDependence;
    public float positionBase;
    public float rotationBase;

    private Rigidbody rig;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rig = targetPosition.parent.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var speed = rig.linearVelocity.magnitude*rig.linearVelocity.magnitude*speedDependence;
        transform.position = Vector3.Slerp(transform.position,targetPosition.position,Mathf.Sqrt(Time.deltaTime*(1-1/(positionBase+speed))));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetPosition.rotation, Mathf.Sqrt(Time.deltaTime*(1-1/(rotationBase+speed))));
    }
}
