using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public List<GameLevel> allLevels = new List<GameLevel>();
    public SceneField mainScene;
    bool mainSceneIsLoaded;

    private void Awake()
    {
        int levelToLoad = PlayerPrefs.GetInt("LevelToLoad");
        LoadLevel(levelToLoad);
    }

    public void LoadLevel(int levelIndex)
    {
        GetComponent<Camera>().enabled = false;
        if (mainSceneIsLoaded == false)
            SceneManager.LoadScene(mainScene, LoadSceneMode.Additive);

        //print("We're going to " + allLevels[levelIndex].levelName);
        SceneManager.LoadScene(allLevels[levelIndex].levelScene, LoadSceneMode.Additive);
    }
}

[System.Serializable]
public class GameLevel
{
    public string levelName;
    public SceneField levelScene;
}

