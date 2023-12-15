using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class GameSceneManager : MonoBehaviour {

	private static GameSceneManager _instance = null;
	public static GameSceneManager Instance
	{
		get
		{
			if(!_instance) _instance = (GameSceneManager) FindObjectOfType(typeof(GameSceneManager));
			return _instance;
	    }
	}


    public Image LevelOveraly = null;
    public List<GameObject> Mazes = new List<GameObject>();
    public List<float> Time = new List<float>();
    public List<float> Time2 = new List<float>();
  
    public List<Vector3>        BallPositions= new List<Vector3>();  //store the start position of the ball for each level. is initialized via the inspector
    public Button PlayButton;
    public Button PreviousMazeButton;
    public Button NextMazeButton;
    public Sprite PauseSprite;
    public Sprite PlaySprite;
    public Sprite ReplaySprite;
    public GameObject MainBall;
    public Text myText;
    public Material normalMaterial;
    public Material passedMaterial;
    public Material myMaterial;
    public Material failedMaterial;
    public Text LevelNumber;
    public Text BonusText;
    public int levelIndex = 0;



    //datas used for coroutines.
    private float amount = 0.05f;
    private GameObject thisMaze = null;
    private GameObject anotherMaze = null;
    private GameState state;
    private GameState tempState;    //retine stadiul jocului atunci cand acesta a fost pauzat. Este modificat in OnApplicationFocus///OnApplicationPause
    private Rigidbody myRigidbody = null;
    private Transform myTransform = null;
    private float gravityForce = 500.0f;
    private float time;
    private float timeElapsed = 0.0f;
    private List<int> passedLevels = new List<int>();
    private float BonusTime;




    void Awake()
    {
        //fade in/out coroutines effect
        Load();
        StopCoroutine("ActivateOveraly");
        StopCoroutine("MoveNextMaze");
        StartCoroutine("DezactivateOveraly");

        //order mazes in the list.
        OrderMazes();

        //keep the screen active.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;


        //initializam timpul;
        time = Time[levelIndex];
        BonusTime = 0.0f;
        SetTime();

        //setez culoarea labirintului din runda curenta
        SetColor();

        //setez pozitia si dimensiunea bilei la inceput
        MainBall.transform.localScale = BallDimension();
        MainBall.transform.position = BallPositions[levelIndex/4];

        thisMaze = Instantiate(Mazes[levelIndex]) as GameObject;
        LevelNumber.text = "LEVEL " + (levelIndex + 1).ToString();

        //la inceputul nivelului setam statutul jocului ca fiind StartState;
        state = tempState= GameState.StartState;
        gravityForce = 350.0f;



        //retinem cateva referinte
        myRigidbody = MainBall.GetComponent<Rigidbody>() as Rigidbody;
        myTransform = MainBall.transform as Transform;


    }


    void OnApplicationPause(bool pauseStatus)
    {
        if(pauseStatus == true)
        {
            tempState = state;
            state = GameState.Pause;
        }
        Save();
    }

    void OnApplicationFocus(bool hasFocused)
    {
        if (hasFocused == true)
            Load();
    }




    void Update()
    {

        if (state == GameState.StartState)          //pentru stadiul StartState
        {
            if (!passedLevels.Contains(levelIndex)) PlayButton.image.sprite = PlaySprite;   //setam imaginea butonului ca fiind play;
            else PlayButton.image.sprite = ReplaySprite;


            PlayButton.interactable = true;                 //butonul play este activ
            PreviousMazeButton.interactable = true;         //butonul catre nivelul precedent este activ


            //daca nivelul curent este completat, butonul catre nivelul urmator este activ
            if (passedLevels.Contains(levelIndex)) NextMazeButton.interactable = true;

            //altfel este inactiv.  
            else NextMazeButton.interactable = false;

            myRigidbody.isKinematic = true;         //nu se aplica gravitatia asupra bilei.
            MainBall.SetActive(false);
        }

        else if (state == GameState.Pause)
        {
            PlayButton.image.sprite = PlaySprite;   //setam imaginea butonului ca fiind play;

            //setam cele 3 butoane ca fiind disponibile
            PlayButton.interactable = true;
            PreviousMazeButton.interactable = true;

            if (passedLevels.Contains(levelIndex)) NextMazeButton.interactable = true;
            else NextMazeButton.interactable = false;
            myRigidbody.isKinematic = true;
        }

        else if (state == GameState.LevelFailed)
        {
            PlayButton.image.sprite = ReplaySprite;

            PlayButton.interactable = true;
            PreviousMazeButton.interactable = true;
            if (passedLevels.Contains(levelIndex)) NextMazeButton.interactable = true;
            else NextMazeButton.interactable = false;

        }

        else if (GameState.LevelCompleted == state)
        {
            PlayButton.image.sprite = ReplaySprite;

            PlayButton.interactable = true;
            NextMazeButton.interactable = true;
            PreviousMazeButton.interactable = true;
        }

        else if (state == GameState.Play)
        {
           /// #if UNITY_ANDROID
                Physics.gravity = Input.acceleration.normalized * gravityForce;
           /// #endif
            //verificam daca s-a completat nivelul curent.
            if (myTransform.position.y <= -39.0f && time >= 0.0f)
            {
                MainBall.SetActive(false);
                state = GameState.LevelCompleted;

                if (!passedLevels.Contains(levelIndex))
                {
                    passedLevels.Add(levelIndex);
                    SetColor();
                }
                if (levelIndex < 95)
                {
                    BonusTime = time;
                    BonusTime = Mathf.RoundToInt(BonusTime);
                    BonusTime = Mathf.Clamp(BonusTime, 0.0f, 3.5f);
                    Time[levelIndex + 1] = Time2[levelIndex + 1] + BonusTime;
                    if (BonusTime >= 1.0f)
                        StartCoroutine("ShowBonusTime");
                }

                GameManagerScript.Instance.PlaySound(1);
                Save();
                return;
            }

            if (time > 0.0f)           //daca nivelul este in desfasurare si timpul nu a ajuns la 0
            {
                timeElapsed += UnityEngine.Time.deltaTime;
                time -= UnityEngine.Time.deltaTime;

                SetTime();

                PlayButton.image.sprite = PauseSprite;

                PlayButton.interactable = true;
                PreviousMazeButton.interactable = false;
                NextMazeButton.interactable = false;
            }

            else                    //daca timpul a expirat si nivelul nu a fost completat.
            {
                MainBall.SetActive(false);
                myTransform.position = BallPositions[levelIndex / 4];
                SetColor();
                //if(GameManagerScript.Instance.SoundState)		Handheld.Vibrate();


                time = Time[levelIndex];
                timeElapsed = 0.0f;
                LevelOveraly.color = Color.clear;
                LevelOveraly.gameObject.SetActive(true);
                Invoke("SetColor", 1.0f);


                state = GameState.LevelFailed;
            }

            if (myRigidbody.isKinematic) myRigidbody.isKinematic = false;
        }


        else if (state == GameState.MoveState)
        {
            PlayButton.image.sprite = PlaySprite;

            PlayButton.interactable = false;
            PreviousMazeButton.interactable = false;
            NextMazeButton.interactable = false;
            myRigidbody.isKinematic = true;
        }


    }

    //calculate the dimension of the ball based on the levelIndex.
    Vector3 BallDimension()
    {
        Vector3 dim = Vector3.zero;
        float x;

        if ((Mazes[levelIndex].name[0] - '0') < 6)
            x = Int32.Parse(string.Concat(Mazes[levelIndex].name[0], Mazes[levelIndex].name[1]));

        else
            x = (Mazes[levelIndex].name[0] - '0');


        x = (x - 53.76f) / (-17.56f);        //the formula with which I calculate the size of the ball based on the levelIndex.
        dim = new Vector3(x, x, x);
        return dim;
    }

    //order mazes in the Mazes list by their difficulty. this function is called once, at level start from awake function.
    private void OrderMazes()
    {
        Mazes.Sort((go1, go2) =>
            {
                int s1, s2, c1, c2;

                if ((go1.name[0] - '0') < 6)
                    s1 = Int32.Parse(string.Concat(go1.name[0], go1.name[1]));
                else s1 = go1.name[0] - '0';

                if ((go2.name[0] - '0') < 6)
                    s2 = Int32.Parse(string.Concat(go2.name[0], go2.name[1]));
                else s2 = go2.name[0] - '0';

                c1 = go1.name[go1.name.Length - 1];
                c2 = go2.name[go2.name.Length - 1];

                if (s1 == s2)
                {
                    if (c1 < c2) return -1;
                    else if (c1 == c2) return 0;
                    else return 1;
                }

                else
                {
                    if (s1 < s2) return -1;
                    else if (s1 == s2) return 0;
                    else return 1;
                }

            }
        );
    }


    /// <summary>
    /// set the color of the maze based on its state:
    /// if it was completed it is green or idk what color
    /// if it was not completed it has another color
    /// so on...
    /// </summary>
	public void SetColor()
    {
        LevelOveraly.gameObject.SetActive(true);
        if (time <= 0.0f && state == GameState.Play)
        {
            myMaterial.SetColor("_Color", failedMaterial.GetColor("_Color"));
            myMaterial.SetColor("_SpecColor", failedMaterial.GetColor("_SpecColor"));
        }
        else if (passedLevels.Contains(levelIndex))
        {
            myMaterial.SetColor("_Color", passedMaterial.GetColor("_Color"));
            myMaterial.SetColor("_SpecColor", passedMaterial.GetColor("_SpecColor"));
        }

        else
        {
            myMaterial.SetColor("_Color", normalMaterial.GetColor("_Color"));
            myMaterial.SetColor("_SpecColor", normalMaterial.GetColor("_SpecColor"));
        }

        LevelOveraly.gameObject.SetActive(false);
    }

    //display text in a nice format.
    public void SetTime()
    {
        string seconds = System.Math.Truncate(time).ToString("##");
        int result = (int)((time - (int)time) * 100);
        myText.text = seconds + ":" + (result / 10).ToString() + "0";
    }

    //Initialize the stuff for going to the next level and then call the MoveNextMaze coroutine
    public void NextMaze()
    {
        StopCoroutine("DezactivateOveraly");
        StopCoroutine("ActivateOveraly");

        LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 0.0f);
        LevelOveraly.gameObject.SetActive(false);

        BonusTime = 0.0f;
        timeElapsed = 0.0f;
        time = Time[levelIndex];


        if (levelIndex + 1 <= Mazes.Count - 1) levelIndex++;
        else return;

        state = GameState.MoveState;
        StartCoroutine("MoveNextMaze");

    }


    //Initialize the stuff for going to the previous level and then call the MovePreviousMaze coroutine
    public void PreviousMaze()
    {
        StopCoroutine("DezactivateOveraly");
        StopCoroutine("ActivateOveraly");

        LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 0.0f);
        LevelOveraly.gameObject.SetActive(false);

        if (levelIndex - 1 >= 0) levelIndex--;
        else return;

        BonusTime = 0.0f;
        timeElapsed = 0.0f;
        time = Time[levelIndex];

        state = GameState.MoveState;
        StartCoroutine("MovePreviousMaze");
    }


    public void Play()
    {
        MainBall.SetActive(true);
        if (PlayButton.image.sprite == PlaySprite)
        {
            PlayButton.image.sprite = PauseSprite;
            state = GameState.Play;
        }

        else if (PlayButton.image.sprite == PauseSprite)
        {
            PlayButton.image.sprite = PlaySprite;
            state = GameState.Pause;
        }

        else
        {
            PlayButton.image.sprite = PauseSprite;
            state = GameState.Play;
            myTransform.position = BallPositions[levelIndex / 4];
            MainBall.SetActive(true);
            timeElapsed = 0.0f;
            time = Time[levelIndex];
        }
    }

    //literally moves the previous maze in the scene
    private IEnumerator MovePreviousMaze()
    {
        MainBall.SetActive(false);
        myTransform.position = BallPositions[levelIndex / 4];
        myTransform.localScale = BallDimension();
        float x = thisMaze.transform.position.x;
        float x2;
        float moveAmount = 3.0f;
        SetColor();

        anotherMaze = Instantiate(Mazes[levelIndex], new Vector3(-63, Mazes[levelIndex].transform.position.y,
                                                                Mazes[levelIndex].transform.position.z), Quaternion.identity) as GameObject;
        //if (!passedLevels.Contains (levelIndex))
        //				anotherMaze.GetComponent<Renderer>().material = normalMaterial;

        x2 = anotherMaze.transform.position.x;

        while (x < 63.0f)
        {
            x += moveAmount;
            x2 += moveAmount;

            thisMaze.transform.position = new Vector3(x, thisMaze.transform.position.y,
                                                      thisMaze.transform.position.z);

            anotherMaze.transform.position = new Vector3(x2, anotherMaze.transform.position.y, anotherMaze.transform.position.z);

            yield return null;
        }

        anotherMaze.GetComponent<Renderer>().material = myMaterial;
        Destroy(thisMaze);
        thisMaze = anotherMaze;
        anotherMaze = null;
        state = GameState.StartState;
        timeElapsed = 0.0f;
        time = Time[levelIndex];
        SetTime();
        LevelNumber.text = "LEVEL " + (levelIndex + 1).ToString();
        //set the ball start position
    }

    //literally moves the next maze in the scene
    private IEnumerator MoveNextMaze()
    {
        MainBall.SetActive(false);
        myTransform.position = BallPositions[levelIndex / 4];
        myTransform.localScale = BallDimension();
        float x = thisMaze.transform.position.x;
        float x2;
        float moveAmount = 3.0f;
        SetColor();

        anotherMaze = Instantiate(Mazes[levelIndex], new Vector3(63.0f, Mazes[levelIndex].transform.position.y,
                                                                Mazes[levelIndex].transform.position.z), Quaternion.identity) as GameObject;
        //if (!passedLevels.Contains (levelIndex))
        //				anotherMaze.GetComponent<Renderer>().material = normalMaterial;

        x2 = anotherMaze.transform.position.x;

        while (x > -63.0f)
        {
            x -= moveAmount;
            x2 -= moveAmount;

            thisMaze.transform.position = new Vector3(x, thisMaze.transform.position.y,
                                                      thisMaze.transform.position.z);

            anotherMaze.transform.position = new Vector3(x2, anotherMaze.transform.position.y,
                                                      anotherMaze.transform.position.z);

            yield return null;
        }

        anotherMaze.GetComponent<Renderer>().material = myMaterial;
        Destroy(thisMaze);
        thisMaze = anotherMaze;
        anotherMaze = null;
        state = GameState.StartState;
        timeElapsed = 0.0f;
        time = Time[levelIndex];
        SetTime();
        LevelNumber.text = "LEVEL " + (levelIndex + 1).ToString();
    }



    public IEnumerator DezactivateOveraly()
    {
        LevelOveraly.gameObject.SetActive(true);
        LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 1.0f);
        float alpha = LevelOveraly.color.a;

        while (alpha > 0.0f)
        {
            alpha -= amount;
            LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, alpha);
            yield return null;
        }

        LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 0.0f);
        LevelOveraly.gameObject.SetActive(false);
    }



    public IEnumerator ActivateOveraly(int levelIndex)
    {
        StopCoroutine("MoveNextMaze");
        StopCoroutine("MovePreviousMaze");
        StopCoroutine("DezactivateOveraly");
        LevelOveraly.gameObject.SetActive(true);
        LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 0.0f);
        float alpha = LevelOveraly.color.a;

        while (alpha < 1.0f)
        {
            alpha += amount;
            LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, alpha);
            yield return null;
        }
        LevelOveraly.color = new Color(LevelOveraly.color.r, LevelOveraly.color.g, LevelOveraly.color.g, 1.0f);
        Save();
        Application.LoadLevel(levelIndex);
    }


    public IEnumerator ShowBonusTime()
    {
        string seconds = System.Math.Truncate(BonusTime).ToString("##");
        int result = (int)((BonusTime - (int)BonusTime) * 100);
        BonusText.text = "+ " + seconds + ":" + (result / 10).ToString() + "0";
        BonusText.rectTransform.anchoredPosition = new Vector2(BonusText.rectTransform.anchoredPosition.x, 238.4f);
        BonusText.gameObject.SetActive(true);
        float t = BonusText.rectTransform.anchoredPosition.y;

        while (t <= 245.6f)
        {
            t += 3.0f;
            BonusText.rectTransform.anchoredPosition = new Vector2(BonusText.rectTransform.anchoredPosition.x, t);
            yield return null;
        }

        while (t <= 280.3f)
        {
            t += 0.7f;
            BonusText.rectTransform.anchoredPosition = new Vector2(BonusText.rectTransform.anchoredPosition.x, t);
            yield return null;
        }

        float f = 0.5f;
        while (f >= 0.0f)
        {
            t += 3.0f;
            BonusText.rectTransform.anchoredPosition = new Vector2(BonusText.rectTransform.anchoredPosition.x, t);
            f -= UnityEngine.Time.deltaTime;
            yield return null;
        }

        BonusText.gameObject.SetActive(false);


    }

    //the datas that needs to be stored before the app being backgrounded. It is called from GameManagerScript, inside the OnApplicationPause method.

    public void SaveTempDatas(out float t, out Vector3 pos, out int s)         
    {
        t = time;
        pos = MainBall.transform.localPosition;
        if(state == GameState.Play) state = GameState.Pause;
        s = (int)state;
    }


    //this method get the datas that weere stored before the app being backgrounded. Now that the app is focused again we need to get those datas back.
    public void GetTempDatas(float t, Vector3 pos, int s)
    {
        time = t;
        MainBall.transform.localPosition = pos;
        state = (GameState)s;
    }




    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/MazeRunnerDatas.dat");

        Datas dt = new Datas();
        dt.i = levelIndex;
        dt.levels.Clear();
        dt.levels.AddRange(passedLevels);

        dt.t.Clear();
        dt.t.AddRange(Time);



        bf.Serialize(file, dt);
        file.Close();
    }


    public void Load()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(Application.persistentDataPath + "/MazeRunnerDatas.dat"))
        {

            FileStream file = File.Open(Application.persistentDataPath + "/MazeRunnerDatas.dat", FileMode.Open);
            Datas dt = (Datas)bf.Deserialize(file);

            levelIndex = dt.i;                       //nivelul curent (nivelul la care playerul a ramas)
            passedLevels.Clear();                    //o lista cu nivelele care au fost deja completate    
            passedLevels.AddRange(dt.levels);

            Time.Clear();                          //o lista cu timpul pentru fiecare nivel
            Time.AddRange(dt.t);


            file.Close();
        }
    }




    public enum GameState
    {
        Play    = 1,               //jocul se afla in stadiul de play. este setat la liniile 314 si 326 de catre functia Play
        Pause,              // jocul se afla in stadiul pauza. este setat la linia 320
        StartState,         //este setat la liniile 78,368,411, atunci cand scena este incarcata(Awake) dupa ce se face tranzitia labirinturilor.
        MoveState,          //se creeaza tranzactia dintre labirinturi. este apelat de catre butoanele PreviousMaze si NextMaze si este setat la liniile 281 si 303
        LevelCompleted,     //nivelul a fost completat este setat la linia 182
        LevelFailed         //nivelul a fost esuat este setat la linia 213
    }
}

[Serializable]
public class Datas
{
    public int i;
    public List<int> levels = new List<int>();
    public List<float> t = new List<float>();
    public List<float> t2 = new List<float>();
}
