using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ProportionedWheel{
	public WheelCollider wheel;
	public float power;
}

public class CarController : MonoBehaviour {

	private WheelCollider[] posableWheels;

	public float acceleration = 300f;
	public float engineBraking = 10f;
	public float breakingForce = 600f;
	public float maxTurnAngle = 25f;
	public float clutchK=1500f;
	private float currentAcceleration = 0f;
	private float currentBreakforce = 0f;
	private float currentTurnAngle = 0f;
	private float currentHandbrake =0f;

	public ProportionedWheel[] steeredWheels;
	public ProportionedWheel[] accelerationWheels/*new { frontLeft, frontRight }*/;
	public ProportionedWheel[] brakingWheels;
	public ProportionedWheel[] handbrakeWheels;


	public float[] gearRatios;
	public float finalRatio;
	public float idleRpm;
	public float redlineRpm;
	public float engineInertia;

	public int currentGear;

	public float currentRpm=0;
	public float commandedTorque=0;
	public float inputRpm=0;
	public float clutchDebug=0;
	public float maxTorqueDebug=0;

	private int changeTimer=0;
	private float transmissionControl()
	{
		changeTimer++;
		var changeTreshold=50;
		//if(changeTimer>100){
		if(commandedTorque>0.7f*acceleration &&
			currentGear < gearRatios.Length-1 &&
			currentRpm > 6000
		) {
			if(changeTimer>changeTreshold && clutchDebug>0.98f){
			currentGear++;
			changeTimer=0;
			}
		}else
		if(commandedTorque>0.7f*acceleration &&
			currentGear>0 &&
			getEngineSpeed(currentGear-1) < 5000)
		{
			if(changeTimer>changeTreshold){
				currentGear--;
				changeTimer=0;
			}
		} else if(commandedTorque>0.1f*acceleration &&
					commandedTorque < 0.5f &&
					currentGear < gearRatios.Length-1 &&
					getEngineSpeed(currentGear+1) > 1800) {
						
			if(changeTimer>changeTreshold){
			currentGear++;
			changeTimer=0;
			} // only driven by grandma
		} else if(commandedTorque<0.05f*acceleration &&
					currentRpm < idleRpm &&
					currentGear> 0) {
			if(changeTimer>changeTreshold){
				currentGear--;
				changeTimer=0;
			}
		}//else
		/*if( currentGear==0 && (commandedTorque<0.05f ||currentRpm<idleRpm))
		{
			return 0.0f;
		}*/
		if(currentRpm>1500)
			return clutchDebug*0.95f+0.05f;
		return clutchDebug*0.95f;

	}

	private float map(float a, float b, float c, float d) {
		return (d-c)*(b-a)/b+c;
	}
	private float maxTorque()
	{
		return maxTorque(currentRpm);
	}
	private float maxTorque(float rpm)
	{
		float maxTorque=0.1f;

		if(rpm>500 && rpm<2000)
		{
			maxTorque = map(500,2000,0.25f,0.5f);
		} else if( rpm<2800) {
			maxTorque = map(2000,2800,0.5f,1.03f);
		} else if( rpm<4000) {
			maxTorque = map(2800,4000,1.03f,1.0f);
		} else if( rpm<6200) {
			maxTorque = map(4000,6200,1.0f,0.75f);
		} else {
			maxTorque = map(6200,redlineRpm,0.75f,0.0f);
		}

		return maxTorque*acceleration;
	}
	private float getEngineSpeed() {
		return getEngineSpeed(currentGear);
	}
	private float getRatio()
	{
		return getRatio(currentGear);
	}
	private float getRatio(int gear) {
		return gearRatios[gear]*finalRatio;
	}
	private float getEngineSpeed(int gear) {
		float wheelSpeed = 0.0f; //rpm
		foreach(var wheel in accelerationWheels) {
			if(float.IsNaN(wheel.wheel.rpm))
				wheelSpeed+=0;
			else
				wheelSpeed+=wheel.wheel.rpm;
		}

		return wheelSpeed*getRatio(gear);
	}
	private float oldinputRpm=0.0f;
	private float drivetrain()
	{
		oldinputRpm=inputRpm;
		inputRpm = getEngineSpeed();
		clutchDebug = transmissionControl();
		maxTorqueDebug = maxTorque();
		var enT = Mathf.Min(maxTorqueDebug,commandedTorque);
		float miu = 0.4f;
		float clampingForce = clutchDebug*clutchK; // TODO change to lower value
		float ro=0.15f,ri=0.1f;
		float maxClutchTorque =
			(ro*ro*ro-ri*ri*ri)/(ro*ro-ri*ri)*
			miu * clampingForce;

		float transmissionTorque=0;
		transmissionTorque=Math.Min(maxClutchTorque,enT);

		if(currentRpm<800)
			enT+=5f;
		else
			enT-=5f;
		var diffTorque = enT-transmissionTorque; //124.93 kgm^2
		currentRpm = currentRpm*(1-clutchDebug)+inputRpm*clutchDebug+diffTorque/engineInertia*0.006f*60*10;
		//currentRpm = inputRpm+diffTorque/engineInertia*0.006f*60*10;
		if(currentRpm<0)
			currentRpm=0;

		//Debug.Log("diffTorque" + diffTorque);

		var wheelTorque = transmissionTorque*getRatio();

		return wheelTorque;
	}

	private void FixedUpdate() {

		currentAcceleration=drivetrain();
		//currentAcceleration=commandedTorque*16;

		// Accelerating
		foreach(var wheel in accelerationWheels) {
			wheel.wheel.motorTorque = currentAcceleration*wheel.power;
		}

		// Braking
		foreach(var wheel in brakingWheels) {
			wheel.wheel.brakeTorque = currentBreakforce*wheel.power;
		}
		// Handbrake
		foreach(var wheel in handbrakeWheels) {
			wheel.wheel.brakeTorque += currentHandbrake*wheel.power;

		}

		foreach(var wheel in steeredWheels) {
			wheel.wheel.steerAngle = currentTurnAngle*wheel.power;
		}
	}
	
	private void poseWheels() {
		foreach(var wheel in posableWheels)
		{
			// Get wheel collider state
			Vector3 position;
			Quaternion rotation;
			foreach(Transform trans in wheel.GetComponent<Transform>())
			{
				wheel.GetWorldPose(out position, out rotation);
				// Set wheel transform state
				trans.position = position;
				trans.rotation = rotation;
			}
		}
	}
	public void accelerate(float howMuch) {
		commandedTorque = howMuch * acceleration;
	}
	public void steer(float howMuch) {
		currentTurnAngle =  howMuch * maxTurnAngle;
	}
	public void brake(float howMuch) {
		currentBreakforce = howMuch * breakingForce;
	}
	public void handBrake(float howMuch) {
		currentHandbrake = howMuch * breakingForce;
	}

	void Start()
    {

		var tempWheels = new HashSet<WheelCollider>();
		foreach(var wheel in accelerationWheels)
			tempWheels.Add(wheel.wheel);
		foreach(var wheel in brakingWheels)
			tempWheels.Add(wheel.wheel);
		foreach(var wheel in handbrakeWheels)
			tempWheels.Add(wheel.wheel);
		posableWheels = new WheelCollider[tempWheels.Count];
		tempWheels.CopyTo(posableWheels);
    }

    // Update is called once per frame
    void Update()
    {
		poseWheels();
    }
}
