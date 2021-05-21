using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    [SerializeField]
    GameObject pausePanel;
    [SerializeField]
    PlayerInput playerInput;
    [SerializeField]
    SceneField titleScreenScene;

    public bool isPaused { get; private set; }
    InputAction pauseAction;

    private void Awake()
    {
        instance = this;
        LoadPosition();
    }

    private void Start()
    {
        pauseAction = playerInput.actions.FindAction("Pause");
    }

    private void Update()
    {
        if (pauseAction.triggered)
            SetPause(!isPaused);
    }

    public void SetPause(bool enable)
    {
        isPaused = enable;

        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void LoadTitleScreen()
    {
        DeletePosition();
        SceneManager.LoadScene(titleScreenScene, LoadSceneMode.Single);
    }

    public void SavePosition()
    {
        PlayerPrefsX.SetVector3("SavedPos", playerInput.transform.position);
        PlayerPrefs.Save();
        //print("Saved");
    }

    public void DeletePosition()
    {
        PlayerPrefs.DeleteKey("SavedPos");
        PlayerPrefs.Save();
    }

    void LoadPosition()
    {
        if (PlayerPrefs.HasKey("SavedPos"))
        {
            playerInput.transform.position = PlayerPrefsX.GetVector3("SavedPos");
            //print("LOADED POSS");
        }
    }
}
