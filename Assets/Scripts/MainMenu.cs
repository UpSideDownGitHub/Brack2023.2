//#define DELETEPLAYERPREFS

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CompilerServices.SymbolWriter;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject startIcon;
    public GameObject exitIcon;
    public TMP_Text easyModeText;
    public bool start;
    public bool easyMode;
    private bool easyModeEnabled = false;

    [Header("Best Time")]
    public TMP_Text timeText;
    public TMP_Text coinsText;
    public TMP_Text deathsText;
    public TMP_Text bestTimeTitle;

    public GameData data;

    public AudioSource source;
    public AudioClip clickSound;

    void Start()
    {
#if DELETEPLAYERPREFS
        PlayerPrefs.DeleteAll();
#endif
        easyModeEnabled = data.easyMode;
        if (easyModeEnabled)
        {
            easyModeText.text = "Enabled";
            bestTimeTitle.text = "Best Time (Easy)";
            var hou = PlayerPrefs.GetFloat("E_timeHou", 99);
            var min = PlayerPrefs.GetFloat("E_timeMin", 99);
            var sec = PlayerPrefs.GetFloat("E_timeSec", 99);
            var coin = PlayerPrefs.GetInt("E_coins", 0);
            var death = PlayerPrefs.GetInt("E_deaths", 0);
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hou, min, sec);
            coinsText.text = coin + "/" + data.totalCoins;
            deathsText.text = death.ToString();
        }
        else
        {
            easyModeText.text = "Disabled";
            bestTimeTitle.text = "Best Time";
            var hou = PlayerPrefs.GetFloat("timeHou", 99);
            var min = PlayerPrefs.GetFloat("timeMin", 99);
            var sec = PlayerPrefs.GetFloat("timeSec", 99);
            var coin = PlayerPrefs.GetInt("coins", 0);
            var death = PlayerPrefs.GetInt("deaths", 0);
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hou, min, sec);
            coinsText.text = coin + "/" + data.totalCoins;
            deathsText.text = death.ToString();
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) // Start
        {
            startIcon.SetActive(true);
            exitIcon.SetActive(false);
            easyModeText.color = Color.white;
            start = true;
            easyMode = false;
            source.PlayOneShot(clickSound);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) // Exit
        {
            startIcon.SetActive(false);
            exitIcon.SetActive(true);
            easyModeText.color = Color.white;
            start = false;
            easyMode = false;
            source.PlayOneShot(clickSound);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) // Start
        {
            startIcon.SetActive(true);
            exitIcon.SetActive(false);
            easyModeText.color = Color.white;
            start = true;
            easyMode = false;
            source.PlayOneShot(clickSound);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) // Easy Mode
        {
            startIcon.SetActive(false);
            exitIcon.SetActive(false);
            easyModeText.color = Color.red;
            easyMode = true;
            source.PlayOneShot(clickSound);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            source.PlayOneShot(clickSound);
            if (easyMode)
            {
                easyModeEnabled = !easyModeEnabled;
                data.easyMode = easyModeEnabled;
                if (easyModeEnabled)
                {
                    easyModeText.text = "Enabled";
                    bestTimeTitle.text = "Best Time (Easy)";
                    var hou = PlayerPrefs.GetFloat("E_timeHou", 0);
                    var min = PlayerPrefs.GetFloat("E_timeMin", 0);
                    var sec = PlayerPrefs.GetFloat("E_timeSec", 0);
                    var coin = PlayerPrefs.GetInt("E_coins", 0);
                    var death = PlayerPrefs.GetInt("E_deaths", 0);
                    timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hou, min, sec);
                    coinsText.text = coin + "/" + data.totalCoins;
                    deathsText.text = death.ToString();
                }
                else
                {
                    easyModeText.text = "Disabled";
                    bestTimeTitle.text = "Best Time";
                    var hou = PlayerPrefs.GetFloat("timeHou", 0);
                    var min = PlayerPrefs.GetFloat("timeMin", 0);
                    var sec = PlayerPrefs.GetFloat("timeSec", 0);
                    var coin = PlayerPrefs.GetInt("coins", 0);
                    var death = PlayerPrefs.GetInt("deaths", 0);
                    timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hou, min, sec);
                    coinsText.text = coin + "/" + data.totalCoins;
                    deathsText.text = death.ToString();
                }
            }
            else if (start)
                {
                    gameObject.SetActive(false);
                    SceneManager.LoadSceneAsync(1);
                }
            else
                Application.Quit();
        }
    }
}
