using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public List<GameObject> cops = new List<GameObject>();
	public float gameTime = 0f, spawnFrequency = 60f, BlinksPerSecond = 2f;
	public float killTime = 0;
	private List<GameObject> spawnPoints;
	private float lastSpawn = 0f;

	private GameObject currentCop = null;
	private bool blinking = false;
	public float timeOverCop = 0f;

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
//				currentCop.GetComponent<SpriteRenderer>().order//
				//currentCop.GetComponent<SpriteRenderer>().
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

		if(currentCop.renderer.bounds.IntersectRay(aim)) 
		{
			timeOverCop+= Time.deltaTime;
			if(timeOverCop > killTime)
			{
				Destroy(currentCop.gameObject);
				blinking = false;
				currentCop = null;
				timeOverCop = 0f;
			}
		}
		else 
		{
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
