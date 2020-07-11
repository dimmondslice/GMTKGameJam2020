using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectScript : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        Debug.Log("You did it");
        SceneManager.LoadScene(sceneName);
    }
}
