using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Rendering;
public class Timing : MonoBehaviour
{
	
	public TextMeshProUGUI total;
	public TextMeshProUGUI partial;

	public int previousGate=0;
	public DateTime previousTime;
	public DateTime T3time;

	public TimeSpan[] spans;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		spans = new TimeSpan[3];
		previousGate=0;
    }

    // Update is called once per frame
    void Update()
    {
		if(T3time!=null)
		{
        	total.SetText(string.Format("{0:mm\\:ss\\.ff}", (DateTime.Now-T3time)));
		}
    }

	public void updateTiming(int gate) {
		var now = DateTime.Now;
		if(gate == 3)
			T3time = now;
		if(previousGate !=0)
		{
			spans[gate-1]=now-previousTime;
		}

		StringBuilder str = new StringBuilder();
		for(int i=0; i<spans.Length;i++) {
			if(spans[i]!=null)
			{
				str.AppendLine(string.Format("{0:mm\\:ss\\.ff}", spans[i]));
			}
			else
			{
				str.AppendLine("");
			}
		}

		partial.SetText(str.ToString());

		previousTime=now;
		previousGate=gate;
	}
}
