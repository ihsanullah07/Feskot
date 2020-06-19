using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject soundPanel;
    public GameObject startGamePanel;

    int counter, counter1;

    //start single player scene 

    public void startGame()
    {
        counter1++;
        if (counter1 % 2 == 1)
        {
            startGamePanel.SetActive(true);
        }
        else
            startGamePanel.SetActive(false);
    }

    public void startSinglePlayer()
    {
        SceneManager.LoadScene("SinglePlayer");
    }

    //for multiplayer game
    public void gotoLobby()
    {
        SceneManager.LoadScene("MultiPlayer");
    }

    public void gotoMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void sound()
    {
        counter++;
        if(counter % 2 == 1)
        {
            soundPanel.SetActive(true);
        }
        else
        {
            soundPanel.SetActive(false);
        }
    }

    public void exitGame()
    {
        Application.Quit();
    }

}
