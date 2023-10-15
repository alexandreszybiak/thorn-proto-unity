using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private PixelCharacter character;
    [SerializeField] private LevelList levelList;

    private int currentLevel = 0;
    void Awake()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelList.levels[currentLevel], LoadSceneMode.Additive);
        loadOperation.completed += OnLoadCompleted;
        character.exitLevel += OnCharacterExitLevel;
        character.died += OnCharacterDied;
    }

    // Update is called once per frame
    private void OnCharacterExitLevel()
    {
        character.gameObject.SetActive(false);
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(levelList.levels[currentLevel]);
        unloadOperation.completed += OnUnloadCompleted;

        currentLevel += 1;
        if (currentLevel == levelList.levels.Count) currentLevel = 0;
    }

    private void OnCharacterDied()
    {
        character.gameObject.SetActive(false);
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(levelList.levels[currentLevel]);
        unloadOperation.completed += OnUnloadCompleted;
    }

    private void OnUnloadCompleted(AsyncOperation op)
    {
        op.completed -= OnUnloadCompleted;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelList.levels[currentLevel], LoadSceneMode.Additive);
        loadOperation.completed += OnLoadCompleted;
        
    }

    private void OnLoadCompleted(AsyncOperation op)
    {
        character.gameObject.SetActive(true);
        op.completed -= OnLoadCompleted;
    }
}

