using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public Canvas gameOverScreen;
    public float gameOverTime = 10f;

    private float gameOverDelta = 0f;
    private bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        gameOverScreen.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Explosion")
        {
            gameOverScreen.gameObject.SetActive(true);
            isGameOver = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isGameOver)
        {
            if(gameOverDelta < gameOverTime)
            {
                gameOverDelta += Time.deltaTime;
            }
            else
            {
                string sceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
