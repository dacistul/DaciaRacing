using UnityEngine;
using UnityEngine.InputSystem;


public class AxisSmoothing : InputProcessor<float>
{
    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<AxisSmoothing>();
    }

    [Tooltip("Number to add to incoming values.")]
    public float weight = 0.99f;

    [Tooltip("Number to add to incoming values.")]
    public bool SnapOnRelease=false;
    public bool SnapOnInversion=false;

    private float currentValue=0;
    public override float Process(float value, InputControl control)
    {
        Debug.Log("Henlo!");

        if(SnapOnInversion && value!=0.0f && value/currentValue<0.0f){
            currentValue=value;

            return currentValue;
        }
        if(SnapOnRelease && Mathf.Abs(value)<0.05f){
            currentValue = 0;

            return currentValue;
        }

        currentValue = currentValue*weight + value*(1-weight);
        return currentValue;
    }
}
