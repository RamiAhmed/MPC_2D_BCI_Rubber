using UnityEngine;
using System.Collections;

public class TireController : MonoBehaviour {

	public GameObject LeftTire = null, RightTire = null;

	private GameController gameController;

	// Use this for initialization
	void Start () {
		if (LeftTire != null) {
			LeftTire.renderer.enabled = false;
		}

		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () {
		if (gameController.currentCop != null) {
			if (gameController.currentCop.transform.position.x < 0f) {
				if (LeftTire.renderer.enabled)
					LeftTire.renderer.enabled = false;

				if (!RightTire.renderer.enabled)
					RightTire.renderer.enabled = true;
			}
			else {
				if (!LeftTire.renderer.enabled) 
					LeftTire.renderer.enabled = true;

				if (RightTire.renderer.enabled)
					RightTire.renderer.enabled = false;
			}
		}
	}
}
