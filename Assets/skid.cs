using UnityEngine;

public class skid : MonoBehaviour
{

    private WheelCollider wheel;
    private AudioSource audio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audio = GetComponent<AudioSource>();
        wheel = transform.parent.GetComponent<WheelCollider>();
    }

    private bool alreadyPlaying = false;
    // Update is called once per frame
    private int timeSinceLastSkid=500;
    void FixedUpdate()
    {
        WheelHit hit;
        if(wheel.GetGroundHit(out hit))
        {
            var sideways = Mathf.Abs(hit.sidewaysSlip);
            var frontBack = Mathf.Abs(hit.forwardSlip);
            sideways+=frontBack/10.0f;
            if(sideways>0.1f)
            {
                timeSinceLastSkid=0;

                audio.volume = Mathf.Clamp(sideways/2+0.1f,0.0f,1.0f)*0.1f;
                audio.pitch = Mathf.Clamp(sideways/2+0.8f,0.0f,1.0f);
            }
            else{
                timeSinceLastSkid++;
            }
        }
        else
        {
            timeSinceLastSkid++;
        }
        bool shouldPlay = timeSinceLastSkid < 10;
        if(!alreadyPlaying && shouldPlay)
            audio.Play();
        else if(alreadyPlaying && !shouldPlay)
            audio.Stop();
        alreadyPlaying = shouldPlay;
    }
}
