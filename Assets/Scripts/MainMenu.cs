using System.Collections;
using System.Collections.Generic;
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

    public GameData data;

    void Start()
    {
        easyModeEnabled = data.easyMode;
        if (easyModeEnabled)
            easyModeText.text = "Enabled";
        else
            easyModeText.text = "Disabled";

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
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) // Exit
        {
            startIcon.SetActive(false);
            exitIcon.SetActive(true);
            easyModeText.color = Color.white;
            start = false;
            easyMode = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) // Start
        {
            startIcon.SetActive(true);
            exitIcon.SetActive(false);
            easyModeText.color = Color.white;
            start = true;
            easyMode = false;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) // Easy Mode
        {
            startIcon.SetActive(false);
            exitIcon.SetActive(false);
            easyModeText.color = Color.red;
            easyMode = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (easyMode)
            {
                easyModeEnabled = !easyModeEnabled;
                data.easyMode = easyModeEnabled;
                if (easyModeEnabled)
                    easyModeText.text = "Enabled";
                else
                    easyModeText.text = "Disabled";
            }
            else if (start)
                SceneManager.LoadSceneAsync(1);
            else
                Application.Quit();
        }
    }
}
