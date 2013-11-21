

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnityOSCListener : MonoBehaviour  {
	
	public float SignalThreshold = 0.5f;
	
	public void OSCMessageReceived(OSC.NET.OSCMessage message){ 
		string address = message.Address;
		ArrayList args = message.Values;
		
		/*
        Debug.Log(address);
        foreach( var item in args){
            Debug.Log(item);
        }
        */
		
		if (address.Equals("sig")) {
			foreach (object signal in args) {
				float value;
				if (float.TryParse(signal.ToString(), out value)) {
					Debug.Log("Signal value: " + value);
				}
				else {
					Debug.LogWarning("Signal was not float");
				}
			}
		}
		else {
			Debug.LogWarning("Received unknown OSC address: " + address);
		}
	}
}
