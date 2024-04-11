using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Board board;
    public Piece piece;
    public GameObject title;
    public GameObject menu;
    public GameObject gameover;
    public bool Pause{get; private set;}

    private void Awake()
    {
        Pause = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!Pause)
            {
                piece.enabled = false;
                menu.SetActive(true);
            }
        }
    }

    public void GameStart()
    {
        board.SpawnPiece();
        title.SetActive(false);
    }

    public void Resume()
    {
        piece.enabled = true;
        menu.SetActive(false);
    }

    public void Restart()
    {
        Pause = false;
        piece.enabled = true;
        menu.SetActive(false);
        gameover.SetActive(false);
        board.Restart();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
