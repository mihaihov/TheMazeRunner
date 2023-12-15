using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

public class SocialNetworks : MonoBehaviour
{
    void Awake()
    {
        FB.Init(FacebookInit, OnHidden);
    }

    void FacebookInit()
    {

    }

    void OnHidden(bool isActive)
    {
        if (!isActive)
        {
            Time.timeScale = 0f;                     //if the game is obscured by other window
            MM_SceneManager.Instance.BLOCK.gameObject.SetActive(true);
        }

        else
        {
            Time.timeScale = 1f;
            MM_SceneManager.Instance.BLOCK.gameObject.SetActive(false);
        }
    }




    public void ShareOnFacebook()
    {
        //List<string> permission = new List<String>();
        //permission.Add("publish_actions");
        //if (!FB.IsLoggedIn)
        //    FB.LogInWithPublishPermissions(permission, null);
        //FB.FeedShare(
        //    link: new Uri("http://goo.gl/TcV7uu"),
        //    linkName: "GOOGLE",
        //    linkCaption: "Check out GOOGLE.RO",
        //    linkDescription: "World's most popular search engine",
        //    picture: new Uri("http://goo.gl/kr5FD1"),
        //    callback: AfterLogInFacebook
        //    );
        //string appId = "615099818641510";
        //string fbURL = "fb://publish/dialog/share";
        //string link = "http://goo.gl/tyQU4i";
        //string name = "GOOGLE";
        //string caption = "Search Engines";
        //string description = "The world's most popular game engine";
        //string picture = "http://goo.gl/jIH8l9";
        //string redirect = "http://www.facebook.com";
        //Application.OpenURL(fbURL +
        //    "?app_id=" + appId
        //    + "&href=" + WWW.EscapeURL(link)
        //    + "&name=" + WWW.EscapeURL(name)
        //    + "&caption=" + WWW.EscapeURL(caption)
        //    + "&description=" + WWW.EscapeURL(description)
        //    + "&picture=" + WWW.EscapeURL(picture)
        //    + "&redirect_uri=" + WWW.EscapeURL(redirect));

        if (Utilities.IsConnected() == false) return;

        MM_SceneManager.Instance.BLOCK.gameObject.SetActive(true);

        FB.ShareLink(
            new Uri("https://play.google.com/store/apps/details?id=com.MIG.TheMazeRunner"),
            "The Maze Runner",
            "Can you solve all mazes? Get the app from the Google Play Store!",
            callback: this.ShareCallBack);

        MM_SceneManager.Instance.BLOCK.gameObject.SetActive(false);
        //Application.OpenURL("fb://profile/################");
        //yield return new WaitForSeconds(1f);
        //if (this.mIsAppLeft)
        //    this.mIsAppLeft = false;
        //else
        //Application.OpenURL("http://www.facebook.com/mypage");

    }

    public void ShareToTW()
    {
        if (Utilities.IsConnected())
        {
            MM_SceneManager.Instance.BLOCK.gameObject.SetActive(true);
            string message = "Can you solve all mazes? Get the app from the Google Play Store! https://play.google.com/store/apps/details?id=com.MIG.TheMazeRunner";
            Application.OpenURL("https://twitter.com/intent/tweet?text="+message.ToString()+"&amp;lang=eng");
        }
    }

    public void ShareOnGPlus()
    {
        if (Utilities.IsConnected())
        {
            MM_SceneManager.Instance.BLOCK.gameObject.SetActive(true);
            Application.OpenURL("https://plus.google.com/share?url=https://play.google.com/store/apps/details?id=com.MIG.TheMazeRunner");
        }
        
    }


    void ShareCallBack(IResult result)
    {
        if (result.Cancelled)
        {
            //Debug.Log("Share Cancelled");
        }
        else if (!string.IsNullOrEmpty(result.Error))
        {
           // Debug.Log("Error on share!");
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            //Debug.Log("Success on share");
        }
    }
}
