using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System;
using Facebook.Unity;

public class MM_UIManager : MonoBehaviour {

    private static MM_UIManager _instance = null;
    public static MM_UIManager Instance
    {
        get
        {
            if (!_instance) _instance = (MM_UIManager)FindObjectOfType(typeof(MM_UIManager));
            return _instance;
        }
    }


    public Animator sharePanelAnim = null;
    public Animator aboutPanelAnim = null;

    public Text soundText = null;

    private bool sharePanelState;
    private bool aboutPanelState;
    private bool settingsPanelState;






    void Awake()
    {

        if (GameManagerScript.Instance.SoundState == true)
            soundText.text = "MUTE";
        else soundText.text = "SOUND";
    }

    //play button is pressed 
	public void PlayButton()
	{
		StartCoroutine (MM_SceneManager.Instance.ActivateOveraly (2));
	}

    //share button is pressed
    public void ShareButton()
    {
        //close other panels
        if (aboutPanelState) { aboutPanelAnim.Play("CloseAboutPanel"); aboutPanelState = false;  }


        if (!sharePanelState) sharePanelAnim.Play("ShowSharePanel");
        sharePanelState = true;
    }

    //close sharepanel button is pressed
    public void CloseSharePanel()
    {
        if(sharePanelState) sharePanelAnim.Play("CloseSharePanel");
        sharePanelState = false;
    }


    //about button is pressed.
    public void OpenAboutPanel()
    {
        //close other panels
        if (sharePanelState) { sharePanelAnim.Play("CloseSharePanel"); sharePanelState = false; }

        if (!aboutPanelState) aboutPanelAnim.Play("ShowAboutPanel");
        aboutPanelState = true;
    }

    public void CloseAboutPanel()
    {
        if (aboutPanelState) aboutPanelAnim.Play("CloseAboutPanel");
        aboutPanelState = false;
    }



    //setting pannel(now is just sound panel. there is no panel)

    public void OpenSettingsPanel()
    {
        if(sharePanelState) { sharePanelAnim.Play("CloseSharePanel"); sharePanelState = false; }

        if (aboutPanelState) { aboutPanelAnim.Play("CloseAboutPanel"); aboutPanelState = false; }

        if (soundText.text.Equals("SOUND"))
        {
            soundText.text = "MUTE";
            GameManagerScript.Instance.SoundState = true;
        }

        else
        {
            soundText.text = "SOUND";
            GameManagerScript.Instance.SoundState = false;
        }
        GameManagerScript.Instance.Save();
    }


    //end of setting panel

        //test


        // endtest



    public void QuitGame()
    {
        GameManagerScript.Instance.QuitGame();
    }


}
