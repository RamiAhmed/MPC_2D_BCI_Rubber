using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public List<GameObject> cops = new List<GameObject>();
	public float gameTime = 0f, spawnFrequency = 60f, BlinksPerSecond = 2f;
	public float killTime = 0;
	
	public Texture2D MindBarTexture = null;
	public Texture2D MindBarContainerTexture = null;

	public AudioClip MindLoadSound = null, DeathSound = null;

	private List<GameObject> spawnPoints;

	public GameObject currentCop = null;
	private bool blinking = false;
	public float timeOverCop = 0f;

	public bool MouseDebugging = false, SignalValueDebug = false;

	private UnityOSCListener listener = null;

	private float lastCopKill = 0f;

	private AudioSource mindLoadSource = null, deathSoundSource = null;

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

		if (MindLoadSound != null) {
			mindLoadSource = this.gameObject.AddComponent<AudioSource>();
			mindLoadSource.clip = MindLoadSound;
			mindLoadSource.playOnAwake = false;
		}

		if (DeathSound != null) {
			deathSoundSource = this.gameObject.AddComponent<AudioSource>();
			deathSoundSource.clip = DeathSound;
			deathSoundSource.playOnAwake = false;
		}
		
		Invoke("spawnCop", 1.5f);
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		gameTime += Time.deltaTime;

		if (currentCop == null) {
			if (gameTime - lastCopKill > spawnFrequency) {
				spawnCop();
			}	
		}
		else {
			switch (CurrentBlinkMode) {
				case BlinkMode.FULL_BODY: currentCop.transform.GetChild(0).renderer.sortingOrder = 3; break;
				case BlinkMode.SILHOUTTE: currentCop.transform.GetChild(0).renderer.sortingOrder = 1; break;
				case BlinkMode.BACKGROUND: break;
			}

			if (!blinking) {
				StartCoroutine(Blink ());
			}

			if (timeOverCop > 0f && timeOverCop < killTime) {
				if (mindLoadSource != null) {
					float pitch = 3f * (timeOverCop/killTime);
					mindLoadSource.pitch = pitch;

					if (!mindLoadSource.isPlaying) {
						mindLoadSource.Play();
					}
				}
			}

			killCop();
		}
	}

	
	void OnGUI() {
		float width = Screen.width * 0.99f,
		height = Screen.height * 0.1f;
		float x = (Screen.width/2f)-(width/2f), y = 5f;
		
		float barWidth = width * (1f - (timeOverCop/killTime));
		
		GUI.BeginGroup(new Rect(x, y, barWidth, height));
			GUI.DrawTexture(new Rect(5f, 0f, width-5f, height), MindBarTexture, ScaleMode.StretchToFill);		
		GUI.EndGroup();

		GUI.DrawTexture(new Rect(x, y, width, height), MindBarContainerTexture, ScaleMode.StretchToFill);

		if (SignalValueDebug) {
			string signalDebug = listener.SignalValue.ToString() + "\n";
			signalDebug += listener.SignalValue < listener.SignalThreshold ? "OFF" : "ON"; 
			Color signalColor = listener.SignalValue < listener.SignalThreshold ? Color.yellow : Color.green;
			GUI.color = signalColor;
			GUI.Box(new Rect(5f, (Screen.height-55f), 100f, 50f), signalDebug); 
		}
	}

	IEnumerator Blink() {
		if (!blinking) {
			blinking = true;
			
			Debug.Log ("BLINK ON " + currentCop);
			float waitTime = 1f / BlinksPerSecond;
			while (currentCop != null) {
				if (currentCop != null) {
					currentCop.transform.GetChild(0).renderer.enabled = false;
				}

				yield return new WaitForSeconds(waitTime);

				if (currentCop != null) {
					currentCop.transform.GetChild(0).renderer.enabled = true;
				}

				yield return new WaitForSeconds(waitTime);
			}
		}
	}

	private void spawnCop() {
		GameObject spawnPoint = getRandomSpawnPoint();
		GameObject cop = getRandomCop();

		GameObject newCop = Instantiate(cop) as GameObject;

		newCop.transform.position = spawnPoint.transform.position;
		newCop.transform.localScale = spawnPoint.transform.localScale;

		currentCop = newCop;
	}

	private void killCop(){
		if (MouseDebugging) {
			Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			Ray aim = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));
			Renderer renderer = currentCop.renderer;
			if (renderer == null || !renderer.enabled) {
				renderer = currentCop.GetComponentInChildren<Renderer>();
			}

			if (renderer.bounds.IntersectRay(aim)) {
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
			blinking = false;
			lastCopKill = gameTime;

			Animator anim = currentCop.GetComponent<Animator>();
			if (anim == null) {
				anim = currentCop.GetComponentInChildren<Animator>();
			}

			if (anim != null) {
				anim.SetBool("Play", true);

				for (int i = 0; i < currentCop.transform.childCount; i++) {
					Transform child = currentCop.transform.GetChild(i);
					if (child.CompareTag("silu")) {
						Destroy(child.gameObject);
						break;
					}
				}
			}
			else {
				Debug.LogWarning("Could not find Animator component for cop: " + currentCop.name);
			}

			if (mindLoadSource != null) {
				mindLoadSource.Stop();
			}

			if (deathSoundSource != null) {
				deathSoundSource.Play();
			}

			Invoke("DestroyCop", 1.1f);
		}
	}

	private void DestroyCop() {
		if (currentCop != null) {
			Destroy(currentCop.gameObject);
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
