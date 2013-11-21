using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public List<GameObject> cops = new List<GameObject>();
	public float gameTime = 0f, spawnFrequency = 60f, BlinksPerSecond = 2f;
	public float killTime = 0;
	
	public Texture2D MindBarTexture = null;
	public Texture2D MindBarContainerTexture = null;

	private List<GameObject> spawnPoints;
	private float lastSpawn = 0f;

	public GameObject currentCop = null;
	private bool blinking = false;
	public float timeOverCop = 0f;

	public bool MouseDebugging = false, SignalValueDebug = false;

	private UnityOSCListener listener = null;

	public enum BlinkMode {
		SILHOUTTE,
		FULL_BODY,
		BACKGROUND,
		NULL
	};

	public BlinkMode CurrentBlinkMode = BlinkMode.NULL;

	// Use this for initialization
	void Start () {
		spawnPoints = new List<GameObject>( GameObject.FindGameObjectsWithTag("sp") );

		listener = this.GetComponent<UnityOSCListener>();
		if (listener == null) {
			listener = this.GetComponentInChildren<UnityOSCListener>();
		}

		spawnCop ();
	}
	
	// Update is called once per frame
	void Update () {
		gameTime += Time.deltaTime;

		if (gameTime - lastSpawn > spawnFrequency) {
			lastSpawn = gameTime;

			if (currentCop == null) {
				spawnCop();
			}
		

		}

		if (currentCop != null) {

			switch (CurrentBlinkMode) {
				case BlinkMode.FULL_BODY: currentCop.transform.GetChild(0).renderer.sortingOrder = 3; break;
				case BlinkMode.SILHOUTTE: currentCop.transform.GetChild(0).renderer.sortingOrder = 1; break;
				case BlinkMode.BACKGROUND: break;
			}

			if (!blinking) {
				StartCoroutine(Blink ());
			}
			killCop();
		}
	}

	
	void OnGUI() {
		float width = Screen.width * 0.99f,
		height = Screen.height * 0.1f;
		float x = (Screen.width/2f)-(width/2f), y = 5f;
		
		float barWidth = width * (1-(timeOverCop/killTime));
		
		GUI.BeginGroup(new Rect(x, y, barWidth, height));
			GUI.DrawTexture(new Rect(0f, 0f, width, height), MindBarTexture, ScaleMode.StretchToFill);		
		GUI.EndGroup();

		GUI.DrawTexture(new Rect(x, y, width, height), MindBarContainerTexture, ScaleMode.StretchToFill);

		if (SignalValueDebug) {
			string signalDebug = listener.SignalValue.ToString() + "\n";
			signalDebug += listener.SignalValue < listener.SignalThreshold ? "OFF" : "ON"; 
			GUI.Box(new Rect(5f, (Screen.height-55f), 100f, 50f), signalDebug); 
		}
	}

	/* blinking cop
	 IEnumerator Blink() {
		if (!blinking) {
			blinking = true;

			Debug.Log ("BLINK ON " + currentCop);
			float waitTime = 1f / BlinksPerSecond;
			while (currentCop != null) {
				Debug.Log ("BLINK");
				currentCop.renderer.enabled = false;
				yield return new WaitForSeconds (waitTime);
				currentCop.renderer.enabled = true;
				yield return new WaitForSeconds (waitTime);
			}
		} else {
			Debug.Log("NOT BLINKING");
		}
	}
	*/

	// blinking silu

	IEnumerator Blink() {
		if (!blinking) {
			blinking = true;
			
			Debug.Log ("BLINK ON " + currentCop);
			float waitTime = 1f / BlinksPerSecond;
			while (currentCop != null) {
				//Debug.Log ("BLINK");
				if(currentCop!=null)
					currentCop.transform.GetChild(0).renderer.enabled = false;
				yield return new WaitForSeconds (waitTime);
				if(currentCop!=null)
					currentCop.transform.GetChild(0).renderer.enabled = true;
				yield return new WaitForSeconds (waitTime);
			}
		} else {
			//Debug.Log("NOT BLINKING");
		}
	}

	private void spawnCop() {
		GameObject spawnPoint = getRandomSpawnPoint();
		GameObject cop = getRandomCop ();

		GameObject newCop = Instantiate (cop) as GameObject;

		newCop.transform.position = spawnPoint.transform.position;
		newCop.transform.localScale = spawnPoint.transform.localScale;

		currentCop = newCop;

	}

	private void killCop(){
		Vector2 mousePos = new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		Ray aim = Camera.main.ScreenPointToRay (new Vector3 (mousePos.x, mousePos.y, 0));

		if (MouseDebugging) {
			if (currentCop.renderer.bounds.IntersectRay(aim)) {
				timeOverCop += Time.deltaTime;
			}
			else {
				timeOverCop -= Time.deltaTime;
				if (timeOverCop < 0f) {
					timeOverCop = 0f;
				}
			}
		}

		if (timeOverCop >= killTime) {
			Destroy(currentCop.gameObject);
			blinking = false;
			currentCop = null;
			timeOverCop = 0f;
		}

	}



	private GameObject getRandomSpawnPoint() {
		return spawnPoints[Random.Range(0, spawnPoints.Count)];
	}

	private GameObject getRandomCop() {
		return cops [Random.Range (0, cops.Count)];
	}
}
