using UnityEngine;

public class TimingGate : MonoBehaviour
{
	public int gateNumber;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	void OnTriggerEnter(Collider collision) {
		
        Debug.Log("Henlo!");
        transform.parent.GetComponent<Timing>().updateTiming(gateNumber);
	}
}
