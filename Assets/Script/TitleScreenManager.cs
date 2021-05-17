using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField]
    SceneField levelManager;
    public void LoadLevelButton(int levelId)
    {
        PlayerPrefs.SetInt("LevelToLoad", levelId);
        PlayerPrefs.Save();
        SceneManager.LoadScene(levelManager, LoadSceneMode.Single);
    }
}
