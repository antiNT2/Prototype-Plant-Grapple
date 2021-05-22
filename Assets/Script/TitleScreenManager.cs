using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField]
    SceneField levelManager;

    private void Start()
    {
        PlayerPrefs.DeleteKey("SavedPos");
        PlayerPrefs.Save();
    }

    public void LoadLevelButton(int levelId)
    {
        PlayerPrefs.SetInt("LevelToLoad", levelId);
        PlayerPrefs.Save();
        SceneManager.LoadScene(levelManager, LoadSceneMode.Single);
    }
}
