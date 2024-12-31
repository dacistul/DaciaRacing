using UnityEngine;

public class collision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //Collider col;
    AudioSource audio;
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision) {

        if (collision.relativeVelocity.magnitude > 2) {
            audio.volume = Mathf.Clamp(collision.relativeVelocity.magnitude/50,0,5);
            audio.Play();
        }

        Debug.Log(collision.collider.name);
    }
}
