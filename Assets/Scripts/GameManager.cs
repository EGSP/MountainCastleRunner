using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public delegate void GameLoopDelegate();
    public event GameLoopDelegate OnGameStartEvent = delegate { };
    public event GameLoopDelegate OnGameStopEvent = delegate { };

    public event GameLoopDelegate OnGameUpdate = delegate { };

    public interface IGameLooped
    {
        void GameStart();
        void GameStop();
        void GameUpdate();
    }


    private bool GameStarted = false;
    private int BestScore = 0;
    private int CurrentScore = 0;

    [SerializeField] private float LevelIncreaseFactor = 0.002f;

    [SerializeField] private Canvas MainMenu;
    [SerializeField] private Canvas GameOver;
    [SerializeField] private Image BlackScreen;
    [SerializeField] private float Fade = 1;
    [SerializeField] private bool IsFade;

    [SerializeField] private TextMeshProUGUI Counter;
    [SerializeField] private TextMeshProUGUI Best;
    [SerializeField] private TextMeshProUGUI Current;


    // Start is called before the first frame update
    void Start()
    {
        BestScore = PlayerPrefs.GetInt("Best");

        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsFade)
        {
            var color = BlackScreen.color;
            color.a += Fade*Time.deltaTime;
            color.a = Mathf.Clamp(color.a, 0, 1);
            BlackScreen.color = color;
        }
        else
        {
            var color = BlackScreen.color;
            color.a -= Fade * Time.deltaTime;
            color.a = Mathf.Clamp(color.a, 0, 1);
            BlackScreen.color = color;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartGame();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StopGame();
        }

        if (GameStarted == true)
            OnGameUpdate?.Invoke();

        SpawnController.instance.AddMaxRow(LevelIncreaseFactor * Time.deltaTime);
    }

    public void StartGame()
    {
        GameStarted = true;
        OnGameStartEvent?.Invoke();

        IsFade = false;
        CurrentScore = 0;

        MainMenu.gameObject.SetActive(false);
        GameOver.gameObject.SetActive(false);
        Counter.gameObject.SetActive(true);
    }

    public void StopGame()
    {
        GameStarted = false;
        OnGameStopEvent();

        IsFade = true;

        GameOver.gameObject.SetActive(true);
        Counter.gameObject.SetActive(false);
        
        if (CurrentScore > BestScore)
        {
            BestScore = CurrentScore;
            PlayerPrefs.SetInt("Best", CurrentScore);
        }

        Best.text = BestScore.ToString();
        Current.text = CurrentScore.ToString();

        Counter.text = "0";
    }

    public void AddPoint()
    {
        CurrentScore++;

        Counter.text = CurrentScore.ToString();
    }

    
}
