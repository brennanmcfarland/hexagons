using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneLoader : MonoBehaviour {

    //static variables stay the same between scenes
    public static int player_barrabbascount = 0;
    public static int player_eaglewarriorcount = 0;
    public static int player_stonegolemcount = 0;
    public static int player_firegolemcount = 0;

    public static int computer_barrabbascount = 0;
    public static int computer_eaglewarriorcount = 0;
    public static int computer_stonegolemcount = 0;
    public static int computer_firegolemcount = 0;
    public static bool randommap = false;

    public void LoadScene()
    {
        SceneManager.LoadScene("testscene");
    }

    public void setPlayerBarrabbasCount(float param)
    {
        player_barrabbascount = (int)param;
    }

    public void setPlayerEagleWarriorCount(float param)
    {
        player_eaglewarriorcount = (int)param;
    }

    public void setPlayerStoneGolemCount(float param)
    {
        player_stonegolemcount = (int)param;
    }

    public void setPlayerFireGolemCount(float param)
    {
        player_firegolemcount = (int)param;
    }

    public void setComputerBarrabbasCount(float param)
    {
        computer_barrabbascount = (int)param;
    }

    public void setComputerEagleWarriorCount(float param)
    {
        computer_eaglewarriorcount = (int)param;
    }

    public void setComputerStoneGolemCount(float param)
    {
        computer_stonegolemcount = (int)param;
    }

    public void setComputerFireGolemCount(float param)
    {
        computer_firegolemcount = (int)param;
    }

    public void setRandomMap(System.Boolean param)
    {
        randommap = (bool)param;
    }
}
