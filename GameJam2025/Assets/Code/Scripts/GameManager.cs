using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;
    public float time;
    public PlayerMovement playerMovement;
    public CloudManager cloudManager;
    public float fallTimer;
    public GameObject gameOverPanel;
    public bool gameStarted;
    public bool enabledGameOver;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        gameStarted = false;
        enabledGameOver = false;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1 && !enabledGameOver)
        {
            cloudManager = GameObject.Find("CLOUD MANAGER").GetComponent<CloudManager>();
            playerMovement = GameObject.Find("PLAYER").GetComponent<PlayerMovement>();
            gameOverPanel = PlayerMovement.gameOverPanel;
        }

        if (gameStarted)
        {
            time += Time.deltaTime;

            if (!PlayerMovement.isGrounded)
            {
                fallTimer += Time.deltaTime;

                if (fallTimer > 6 && !enabledGameOver)
                {
                    EnableGameOverScreen();
                    enabledGameOver = true;
                }
            }

            if (PlayerMovement.isGrounded)
            {
                fallTimer = 0;
            }
        }
    }

    public void OnStart()
    {
        gameStarted = true;
    }

    public void EnableGameOverScreen()
    {
        InputManager.PlayerInput.actions.Disable();
        cloudManager.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);

        TextMeshProUGUI text = GameObject.Find("AAAA").GetComponent<TextMeshProUGUI>();

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        PlayerPrefs.SetString("HIGHSCORE_TIME", (string.Format("{0:00}:{1:00}", minutes, seconds)));

        text.text = $"You survived {PlayerPrefs.GetString("HIGHSCORE_TIME")} against Player 2";
    }
}
