using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Players")]
    public GameObject[] players;
    public int whoHoldingBomb;
    public Text timeLabel, startTimerText;
    public Text winPlayerName;
    public GameObject winLabel, startLabel;
    
    [Header("Items")]
    public GameObject[] items;
    public Vector3[] spawnPoints;
    public float spawnCooldown;
    public List<bool> itemSpawnOccupied;

    [Header("Map")]
    public Tilemap gimmick;
    public GameObject audioManagerPrefab;

    [HideInInspector] public bool isPlayable = false;

    public float gameTime = 120;
    private scrpt_AudioManager audioManager;

    private void Awake()
    {
        instance = this;

        // audio
        if (FindObjectsOfType<scrpt_AudioManager>().Length < 1)
        {
            GameObject NewAudioManager = Instantiate(audioManagerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        }
    }

    private void Start()
    {
        isPlayable = false;
        Time.timeScale = 1;
        if (Random.Range(0, 2) % 2 == 0) whoHoldingBomb = 0;
        else whoHoldingBomb = 1;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            itemSpawnOccupied.Add(false);
        }

        audioManager = FindObjectOfType<scrpt_AudioManager>();

        StartCoroutine(SpawnItem());
        DoThePass();
        StartCoroutine(StartCounter());
        audioManager.Play("startCount");
        startLabel.SetActive(true);

        // Time and shit
        int min = Mathf.FloorToInt(gameTime / 60);
        int sec = Mathf.FloorToInt(gameTime % 60);
        if (sec >= 10) timeLabel.text = $"{min}:{sec}";
        else timeLabel.text = $"{min}:0{sec}";

        //print(whoHoldingBomb);
    }

    private float passCooldown = 0f;
    private bool oneTime = true;
    private void Update()
    {
        // Gimmick Change Stuff
        GimmickBlinking();

        // Pass Stuff
        if (passCooldown >= .5f && passTheBomb)
        {
            DoThePass();
            passCooldown = 0;
        }
        else passCooldown += Time.deltaTime;
        passTheBomb = false;

        // Win-Lose Checkout
        if (gameTime <= 0)
        {
            if (!oneTime) return;

            audioManager.Stop("cave bgm");
            audioManager.Stop("grass bgm");

            oneTime = false;
            StartCoroutine(DisplayWinScreen());
            players[whoHoldingBomb].GetComponent<PlayerMovement>().ActivateLoserProtocal();

            timeLabel.GetComponent<Animator>().SetBool("VeryExcitingEffect", false);
            timeLabel.text = "TIME'S UP!";
            timeLabel.color = Color.yellow;
            CameraZoomer.instance.focusTarget = players[whoHoldingBomb].transform;
            
            return;
        }

        if (!isPlayable) return;

        // Time and shit
        if (gameTime < 60) timeLabel.color = Color.red;
        if (gameTime <= 30 && oneTimeToPlayTicking)
        {
            oneTimeToPlayTicking = false;
            timeLabel.GetComponent<Animator>().SetBool("VeryExcitingEffect", true);
            audioManager.Play("ticking");
        }
        int min = Mathf.FloorToInt(gameTime / 60);
        int sec = Mathf.FloorToInt(gameTime % 60);
        if(sec >= 10) timeLabel.text = $"{min}:{sec}";
        else timeLabel.text = $"{min}:0{sec}";
        gameTime -= Time.deltaTime;
    }
    private bool oneTimeToPlayTicking = true;
    private bool passTheBomb = false;
    public void PassBomb()
    {
        bool isPlayer1ThrowingBomb = players[0].GetComponent<PlayerMovement>().isThrowingBomb;
        bool isPlayer2ThrowingBomb = players[1].GetComponent<PlayerMovement>().isThrowingBomb;
        if (!isPlayer1ThrowingBomb && !isPlayer2ThrowingBomb) passTheBomb = true;
    }

    public void DoThePass()
    {
        audioManager.Play("passBomb");

        if (whoHoldingBomb == 1) whoHoldingBomb = 0;
        else whoHoldingBomb = 1;

        foreach (GameObject player in players) player.GetComponent<PlayerMovement>().holdingBomb = false;
        players[whoHoldingBomb].GetComponent<PlayerMovement>().holdingBomb = true;
    }

    int testcase = 1000;

    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(spawnCooldown);
        testcase = 1000;

        while (testcase > 0) 
        { 
            int itemId = Random.Range(0, items.Length);
            int spawnId = Random.Range(0, spawnPoints.Length);

            testcase--;
            //print($"ItemID : {spawnId}");
            if (itemSpawnOccupied[spawnId]) continue;

            spawnCooldown -= .5f;
            GameObject newItem = Instantiate(items[itemId], spawnPoints[spawnId], Quaternion.identity) as GameObject;
            newItem.name = items[itemId].name;
            newItem.GetComponent<Item>().id = spawnId;
            itemSpawnOccupied[spawnId] = true;
            break;
        }
        if (gameTime >= 0) StartCoroutine(SpawnItem());
    }
    private float gimmickTimer = 0;
    private bool onOff = false;
    private void GimmickBlinking()
    {
        if (gimmickTimer > 5)
        {
            audioManager.Play("gimmick");

            if (onOff)
            {
                // ON
                gimmick.color = Color.white;
                gimmick.GetComponent<TilemapCollider2D>().enabled = true;
            }
            else
            {
                // OFF
                gimmick.color = new Color(.5f, .5f, .5f);
                gimmick.GetComponent<TilemapCollider2D>().enabled = false;
            }

            onOff = !onOff;
            gimmickTimer = 0;
        }
        else gimmickTimer += Time.deltaTime;
    }

    private IEnumerator DisplayWinScreen()
    {
        yield return new WaitForSeconds(3);

        int winner;
        if (whoHoldingBomb == 1) winner = 0;
        else winner = 1;
        CameraZoomer.instance.focusTarget = players[winner].transform;

        players[winner].GetComponent<PlayerMovement>().ActivateToxicPlayerProtocal();

        yield return new WaitForSeconds(2);
        timeLabel.gameObject.SetActive(false);
        Time.timeScale = 0;

        if (whoHoldingBomb == 0)
        {
            winPlayerName.text = "RED WINS!";
            winPlayerName.color = Color.red;
        }
        else
        {
            winPlayerName.text = "GREEN WINS!";
            winPlayerName.color = Color.green;
        }
        winLabel.SetActive(true);
    }

    private int startTimer = 3;

    private IEnumerator StartCounter()
    {
        if (startTimer > 0)
        {
            startTimerText.text = startTimer.ToString();
            startTimerText.color = Color.white;
            startTimer--;

            yield return new WaitForSeconds(1);
            StartCoroutine(StartCounter());
        }
        else
        {
            startTimerText.text = "START!";
            startTimerText.color = Color.green;

            yield return new WaitForSeconds(1);
            isPlayable = true;
            startLabel.SetActive(false);
            StopCoroutine(StartCounter());
        }
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
