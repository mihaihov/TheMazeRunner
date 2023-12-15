using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class MM_SceneManager : MonoBehaviour {
	
	private static MM_SceneManager _instance = null;
	public static MM_SceneManager Instance
	{
		get
		{
			if(!_instance)	_instance = (MM_SceneManager)FindObjectOfType(typeof(MM_SceneManager));
			return _instance;
		}
	}

    
    public GameObject BLOCK = null;



	private float amount = 0.05f;


	public Image 					LevelOveraly;




	void Awake()
	{
		StopCoroutine ("ActivateOveraly");
        StartCoroutine("DezactivateOveraly");

        //turn the screen off after 20 secs since the last input from the user.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

	private IEnumerator DezactivateOveraly()
	{
		LevelOveraly.gameObject.SetActive (true);
		LevelOveraly.color = new Color (LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 1.0f);
		float alpha = LevelOveraly.color.a;

		while(alpha>0.0f)
		{
			alpha -= amount;
			LevelOveraly.color = new Color (LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, alpha);
			yield return null;
		}

		LevelOveraly.color = new Color (LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 0.0f);
		LevelOveraly.gameObject.SetActive (false);
	}


	public IEnumerator ActivateOveraly(int levelIndex)
	{
		LevelOveraly.gameObject.SetActive (true);
		LevelOveraly.color = new Color (LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 0.0f);
		float alpha = LevelOveraly.color.a;

		while(alpha<1.0f)
		{
			alpha += amount;
			LevelOveraly.color = new Color (LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, alpha);
			yield return null;
		}
		LevelOveraly.color = new Color (LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 1.0f);
				Application.LoadLevel (levelIndex);

	}
}
