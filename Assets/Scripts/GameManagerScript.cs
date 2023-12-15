using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class GameManagerScript : MonoBehaviour {

	private static GameManagerScript _instance = null;
	public static GameManagerScript Instance
	{
		get 
		{
			if(!_instance)	_instance = (GameManagerScript)FindObjectOfType(typeof(GameManagerScript));
			return _instance;
		}
	}


	private			AudioSource		aSource		=			null;


	public			List<AudioClip> audioFiles	=			null;
	public			Image			MIGLogo		=			null;
	public			bool			SoundState	=			true;
	public 			List<float>		stats 		=   		new List<float>();
    public          string          lastSceneName =         null;


	void Awake()
	{
		Load ();

		//initializarea variabilelor;
		aSource = GetComponent<AudioSource>();
	}


    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
        {
            Time.timeScale = 0.0f;
            lastSceneName = Application.loadedLevelName;        //retin numele ultimului nivel inainte ca aplicatia sa fie backgrounded.

            if(lastSceneName.Equals("MainMenuScene"))           // in cazul in care ultimul nivel este MainMenuScene trebuie inchise toate panourile
            {
                MM_UIManager.Instance.CloseAboutPanel();
                MM_UIManager.Instance.CloseSharePanel();
            }

            else if(lastSceneName.Equals("GameScene"))      //trebuie sa retin pozitia mingii, stadiul jocului(GameState enum) , timpul ramas.
            {
                float t;         Vector3 pos;           int s;
                GameSceneManager.Instance.SaveTempDatas(out t, out pos, out s);
                PlayerPrefs.SetFloat("time", t);
                PlayerPrefs.SetFloat("xPos",pos.x); PlayerPrefs.SetFloat("yPos", pos.y); PlayerPrefs.SetFloat("zPos", pos.z);
                PlayerPrefs.SetInt("state", s);
            }

            PlayerPrefs.SetString("levelName", lastSceneName);
        }

        else
        {
            Time.timeScale = 1.0f;
            try
            {
                lastSceneName = (string)PlayerPrefs.GetString("levelName");
                if(lastSceneName.Equals("GameScene"))
                {
                    float t = PlayerPrefs.GetFloat("time");
                    float x = PlayerPrefs.GetFloat("xPos"); float y = PlayerPrefs.GetFloat("yPos"); float z = PlayerPrefs.GetFloat("zPos");
                    int s = PlayerPrefs.GetInt("state");
                    GameSceneManager.Instance.GetTempDatas(t, new Vector3(x, y, z), s);
                }
                    
            }

            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    void OnApplicationFocus(bool hasFocused)
    {
        if (hasFocused == true)
        {
            if (MM_SceneManager.Instance)
                MM_SceneManager.Instance.BLOCK.gameObject.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }


    void Start()
	{
		DontDestroyOnLoad (this.gameObject);
		if(string.Equals(Application.loadedLevelName,"MIGScene")) StartCoroutine(ShowMIGLogo ());
	}

	void Update()
	{
        //if back button from android is pressed
			if (Input.GetKeyDown (KeyCode.Escape))
			{
            Save();
            if (Application.loadedLevel == 1 || Application.loadedLevel == 0 )
					QuitGame();

				

				else
					StartCoroutine(GameSceneManager.Instance.ActivateOveraly(1));

                
            }
	}




    //create the fade in/out effect for MIG Logo
	private IEnumerator ShowMIGLogo()
	{
		float fadeAmount = 0.008f;
		float timeToWait = 1.5f;
		float alpha = MIGLogo.color.a;
		PlaySound (0);

		while(alpha <= 1.0f)
		{
			alpha += fadeAmount;
			MIGLogo.color = new Color(MIGLogo.color.r,MIGLogo.color.g,MIGLogo.color.b,alpha);
			yield return null;
		}

		MIGLogo.color = new Color(MIGLogo.color.r,MIGLogo.color.g,MIGLogo.color.b,1.0f);

		while(timeToWait >=0)
		{
			timeToWait -= Time.deltaTime;
			yield return null;
		}

		while(alpha >= 0.0f)
		{
			alpha -= fadeAmount;
			MIGLogo.color = new Color(MIGLogo.color.r,MIGLogo.color.g,MIGLogo.color.b,alpha);
			yield return null;
		}
		MIGLogo.color = new Color(MIGLogo.color.r,MIGLogo.color.g,MIGLogo.color.b,0.0f);
		StopSound ();
		Application.LoadLevel (1);
	}


	public void PlaySound(int i, float volume = 1f)
	{
		if(i<0 || i> audioFiles.Count || !aSource || !SoundState)			return;			

		aSource.Stop ();
		aSource.playOnAwake = false;
		aSource.loop = false;
		aSource.volume  = volume;
		aSource.clip = audioFiles[i];
		aSource.Play ();
	}

	public void StopSound()
	{
		aSource.Stop ();
	}


	public void QuitGame()
	{
		Application.Quit ();
	}


    //salveaza valoarea sunetului. 
	public void Save()
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/GameInternal.dat");
		
		InternalData dt = new InternalData ();
		dt.Sound = SoundState;
		
		bf.Serialize (file, dt);
		file.Close ();
	}


    //citeste din fisier valoarea sunetului. daca e false atunci e mut, altfel are sunet.
	public void Load()
	{
		BinaryFormatter bf = new BinaryFormatter ();
		InternalData dt = new InternalData ();
		if(File.Exists(Application.persistentDataPath + "/GameInternal.dat"))
		{
			FileStream file = File.Open (Application.persistentDataPath + "/GameInternal.dat",FileMode.Open);
			dt = (InternalData)bf.Deserialize(file);
			
			SoundState = dt.Sound;
			
			file.Close ();
		}
	}
	
}

[Serializable]
public class InternalData
{
	public bool Sound;
}
