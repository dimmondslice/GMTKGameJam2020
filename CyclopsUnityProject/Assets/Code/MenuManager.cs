using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu, levelMenu, optionsMenu;
    GameObject activeMenu;

    private void Start()
    {
        mainMenu.SetActive(false);
        levelMenu.SetActive(false);
        optionsMenu.SetActive(false);

        activeMenu = mainMenu;
        ChangeMenu(activeMenu);
    }
    public void ChangeMenu(GameObject newMenu)
    {
        activeMenu.SetActive(false);
        newMenu.SetActive(true);
        activeMenu = newMenu;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
