using UnityEngine;

public class EngineSound : MonoBehaviour
{

    public float referenceRPM=800;

    private AudioSource audio;
    private CarController car;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audio = GetComponent<AudioSource>();
        car = transform.parent.GetComponent<CarController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        audio.pitch = car.currentRpm/referenceRPM;
    }
}
