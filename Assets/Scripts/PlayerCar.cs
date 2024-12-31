using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
public class PlayerCar : MonoBehaviour
{
    public bool arcade = true;
	private Rigidbody rig;
    private CarController controller;


	public AxisSmoothing steering;

	//public Transform speedGaugeTransform;
	public TextMeshProUGUI speedGauge;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		rig = GetComponent<Rigidbody>();
        controller = GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {

        controller.steer(steering.Process(rawSteering,null));
		speedGauge.SetText((int)(rig.linearVelocity.magnitude*3.6f) + " km/h");
    }

	private float rawSteering;
    public void OnSteering(InputAction.CallbackContext context) {
        rawSteering = context.ReadValue<float>();
    }
    public void OnHandBrake(InputAction.CallbackContext context) {
        var val = context.ReadValue<float>();
        controller.handBrake(val);
    }
    public void OnBrake(InputAction.CallbackContext context) {
        brake = context.ReadValue<float>();
        if(arcade)
            arcadeControls();
        else
            controller.brake(brake);
    }
    public void OnAccelerator(InputAction.CallbackContext context) {
        accel = context.ReadValue<float>();
        if(arcade)
            arcadeControls();
        else
            controller.accelerate(accel);
    }
    private float accel,brake;
    private void arcadeControls()
    {
        var fbRaw = accel-brake;
        var localVelocity = Quaternion.Inverse(transform.rotation) * rig.linearVelocity;
		if(localVelocity.z==0.0 || localVelocity.z/fbRaw>-0.1f)
		{
			controller.accelerate(fbRaw);
			controller.brake(0.0f);
		} else {
			controller.accelerate(0.0f);
			controller.brake(Mathf.Abs(fbRaw));

		}
    }

	
}
