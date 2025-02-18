﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameController : MonoBehaviour {

	public List<GameObject> cops = new List<GameObject>();
	public float gameTime = 0f, spawnFrequency = 60f, BlinksPerSecond = 2f, killTime = 0f, timeOverCop = 0f;
	
	public Texture2D MindBarTexture = null, MindBarContainerTexture = null;

	public AudioClip MindLoadSound = null, DeathSound = null;

	public enum BlinkMode {
		SILHOUTTE,
		FULL_BODY
	};
	
	public BlinkMode CurrentBlinkMode = BlinkMode.SILHOUTTE;

	public GameObject currentCop = null;

	public bool MouseDebugging = false, SignalValueDebug = false;

	private List<GameObject> spawnPoints;

	private bool blinking = false;

	private UnityOSCListener listener = null;

	private float lastCopKill = 0f;

	private AudioSource mindLoadSource = null, deathSoundSource = null;

	private float copAliveTime = 0f;
	private int copCount = 0;

	// Use this for initialization
	void Start () {
		spawnPoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("sp"));

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

		startNewFileSection();
	}

	private Renderer getSillhoutteRenderer() {
		if (currentCop != null) {
			for (int i = 0; i < currentCop.transform.childCount; i++) {
				if (currentCop.transform.GetChild(i).CompareTag("silu")) {
					if (currentCop.transform.GetChild(i).renderer != null) {
						return currentCop.transform.GetChild(i).renderer;
					}
				}
			}
		}

		//Debug.LogWarning("Could not find Sillhoutte renderer for CurrentCop: " + currentCop.name);
		return null;
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) 
			Application.Quit();
	}

	void FixedUpdate() {
		gameTime += Time.deltaTime;

		if (currentCop == null) {
			if (gameTime - lastCopKill > spawnFrequency) {
				spawnCop();
			}	
		}
		else {
			copAliveTime += Time.deltaTime;

			Renderer silu = getSillhoutteRenderer();
			if (silu != null) {
				switch (CurrentBlinkMode) {
					case BlinkMode.FULL_BODY: silu.sortingOrder = 3; break;
					case BlinkMode.SILHOUTTE: silu.sortingOrder = 1; break;
				}

				if (!blinking) {
					StartCoroutine(Blink());
				}
			}
			
			if (timeOverCop > 0f && timeOverCop <= killTime) {
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
			string signalDebug = listener.SignalValue.ToString("F4") + "\n";
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
				Renderer silu = getSillhoutteRenderer();
				if (currentCop != null && silu != null) {
					silu.enabled = false;
				}
				yield return new WaitForSeconds(waitTime);

				if (currentCop != null && silu != null) {
					silu.enabled = true;
				}
				yield return new WaitForSeconds(waitTime);

				if (!blinking || silu == null) {
					break;
				}
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

		copCount++;
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

			if (!Screen.showCursor) 
				Screen.showCursor = true;
		}
		else {
			if (Screen.showCursor)
				Screen.showCursor = false;
		}

		if (timeOverCop >= killTime) {
			blinking = false;

			Animator anim = currentCop.GetComponent<Animator>();
			if (anim == null) {
				anim = currentCop.GetComponentInChildren<Animator>();
			}

			if (anim != null) {
				if (!anim.GetBool("Play")) 
					anim.SetBool("Play", true);

				if (getSillhoutteRenderer() != null) {
					Destroy(getSillhoutteRenderer().gameObject);
				}
			}
			else {
				Debug.LogWarning("Could not find Animator component for cop: " + currentCop.name);
			}

			if (mindLoadSource != null) {
				if (mindLoadSource.isPlaying) {
					mindLoadSource.Stop();
				}
			}

			if (deathSoundSource != null) {
				if (!deathSoundSource.isPlaying) {
					deathSoundSource.Play();
				}
			}

			Invoke("DestroyCop", 1.1f);
		}
	}

	private void DestroyCop() {
		if (currentCop != null) {
			writeTimeToFile();

			Destroy(currentCop.gameObject);
			currentCop = null;
			timeOverCop = 0f;
			lastCopKill = gameTime;
			copAliveTime = 0f;
		}
	}

	private void writeTimeToFile() {
		string path = Application.dataPath + "/data/";
		string filename = "times.txt";
		string text = "Cop " + copCount.ToString() + " lived for " + copAliveTime + " seconds." + System.Environment.NewLine;

		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}

		System.IO.File.AppendAllText(path + filename, text, System.Text.Encoding.UTF8);
	}

	private void startNewFileSection() {
		string path = Application.dataPath + "/data/";
		string filename = "times.txt";
		string text = System.Environment.NewLine + "---------------------------------------------------------" + System.Environment.NewLine;

		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
		
		System.IO.File.AppendAllText(path + filename, text, System.Text.Encoding.UTF8);
	}

	private GameObject getRandomSpawnPoint() {
		return spawnPoints[Random.Range(0, spawnPoints.Count)];
	}

	private GameObject getRandomCop() {
		return cops[Random.Range(0, cops.Count)];
	}
}
