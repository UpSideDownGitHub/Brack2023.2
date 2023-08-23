using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;
    public bool leftActive = true;

    [Header("UI Elements")]
    public GameData data;
    public TMP_Text timeText;
    public TMP_Text coinsText;
    public TMP_Text deathsText;

    public GameObject newBestTime;

    void OnEnable()
    {
        timeText.text = string.Format("{0:00}:{1:00}:{2:00}", data.hou, data.min, data.sec);
        coinsText.text = data.currentCollectedCoins + "/" + data.totalCoins;
        deathsText.text = data.currentDeaths.ToString();

        if (saveData())
            newBestTime.SetActive(true);
    }

    public bool saveData()
    {
        if (PlayerPrefs.GetFloat("timeHou", 999) > data.hou)
        {
            SaveNewScore();
            return true;
        }
        else if (PlayerPrefs.GetFloat("timeMin", 999) > data.min)
        {
            SaveNewScore();
            return true;
        }
        else if (PlayerPrefs.GetFloat("timeSec", 999) > data.sec)
        {
            SaveNewScore();
            return true;
        }
        return false;
    }

    public void SaveNewScore()
    {
        PlayerPrefs.SetFloat("timeHou", data.hou);
        PlayerPrefs.SetFloat("timeMin", data.min);
        PlayerPrefs.SetFloat("timeSec", data.sec);
        PlayerPrefs.SetInt("coins", data.currentCollectedCoins);
        PlayerPrefs.SetInt("deaths", data.currentDeaths);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            rightButton.SetActive(false);
            leftButton.SetActive(true);
            leftActive = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rightButton.SetActive(true);
            leftButton.SetActive(false);
            leftActive = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1f;
            if (leftActive)
            {
                // return to the menu
                SceneManager.LoadSceneAsync(0);
            }
            else
            {
                // restart the current level
                SceneManager.LoadSceneAsync(1);
            }
        }
    }
}
