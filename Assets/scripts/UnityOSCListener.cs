

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnityOSCListener : MonoBehaviour  {


	public string OSCAddressName = "sig";
	public float SignalThreshold = 0.5f;

	public float SignalValue = 0f;

	private GameController gameController = null;

	void Start() {
		gameController = this.GetComponent<GameController>();
		if (gameController == null) {
			gameController = this.GetComponentInChildren<GameController>();
		}
	}
	
	public void OSCMessageReceived(OSC.NET.OSCMessage message){ 
		string address = message.Address;
		ArrayList args = message.Values;

		if (address.Equals("/" + OSCAddressName)) {
			foreach (object signal in args) {
				float value;
				if (float.TryParse(signal.ToString(), out value)) {
					//Debug.Log("Signal value: " + value);
					SignalValue = value;
				}
				else {
					Debug.LogWarning("Signal was not float, value: " + signal.ToString());
				}
			}
		}
		else {
			Debug.LogWarning("Received unknown OSC address: " + address);
		}
	}

	void FixedUpdate() {
		if (SignalValue > 0f && gameController.currentCop != null) {
			if (SignalValue > SignalThreshold) {
				if (gameController.timeOverCop <= gameController.killTime) {
					gameController.timeOverCop += Time.deltaTime;
				}
			}
			else {
				if (gameController.timeOverCop > 0) {
					gameController.timeOverCop -= Time.deltaTime;
				}
			}
		}
	}
}
