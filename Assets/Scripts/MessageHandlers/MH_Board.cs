﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ResistileClient;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts;
using System;

public class MH_Board : MonoBehaviour, MessageHanderInterface {
    private int msgFromThread = -1;
    private ResistileMessage messageFromThread;
    private System.Object thisLock = new System.Object();
    private float pingWaitingTimer = 5.0f;

    // Use this for initialization
    void Start () {
        GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>().messageInterface = this;
        gameLoaded();
    }

    public void doAction(ResistileMessage message)
    {
        messageFromThread = message;
        lock (thisLock)
        {
            switch (message.messageCode)
            {
                case ResistileMessageTypes.initializeGame:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.tilePlaced:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.drawTile:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.validMove:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.invalidMove:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.gameOver:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.gameResults:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.replay:
                    msgFromThread = message.messageCode;
                    break;
                case ResistileMessageTypes.opponentQuit:
                    msgFromThread = message.messageCode;
                    break;
                default:
                    Debug.Log("Unrecognized Message Type: " + message.messageCode + " --- " + message.message);
                    break;

            }
        }
        
    }

    void Update()
    {
        lock (thisLock)
        {
            if (msgFromThread == ResistileMessageTypes.initializeGame)
            {
                initializeGame(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.tilePlaced)
            {
                tilePlaced(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.drawTile)
            {
                drawTile(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.validMove)
            {
                validMove(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.invalidMove)
            {
                invalidMove(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.gameOver)
            {
                gameOver(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.gameResults)
            {
                gameResults(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.replay)
            {
                replay(messageFromThread);
            }
            else if (msgFromThread == ResistileMessageTypes.opponentQuit)
            {
                opponentQuit(messageFromThread);
            }
            msgFromThread = -1;
            if (pingWaitingTimer <= 0)
            {
                ping();
                pingWaitingTimer = 5.0f;
            }
            else pingWaitingTimer -= Time.deltaTime;
        }
        
    }

    //RECEIVE MESSAGES FROM SERVER
    private void initializeGame(ResistileMessage message)
    {
        //Fill player hand
        for(int i = 0; i < 5; i++)
        {
            GameHandler.gameHandler.Draw((int)message.PlayerHand[i]);
        }
        for (int j = 0; j < 5; j++)
        {
            GameHandler.gameHandler.Draw((int)message.WireHand[j]);
        }
        //Set Objectives
        GameHandler.gameHandler.setPrimaryObj((double)message.PrimaryObjective);
        Debug.Log("Secondary Objectives = " + (int)message.secondaryObjectives[0] + ", " + (int)message.secondaryObjectives[1]);
        GameHandler.gameHandler.setSecondaryObjs((int)message.secondaryObjectives[0], (int)message.secondaryObjectives[1]);
        //Opponent Name
        GameHandler.gameHandler.opponentName = message.message;
        //Set Turn
        GameHandler.gameHandler.initializeTurn(message.turn);
        GameHandler.gameHandler.setTurn();
        GameHandler.gameHandler.gameID = message.gameID;
        GameHandler.gameHandler.setAllTileDrag(GameHandler.gameHandler.isTurn);
    }

    private void tilePlaced(ResistileMessage message) //Opponent Ends Turn
    {
        GameHandler.gameHandler.placeTile(message.tileID, (int)message.coordinates[0], (int)message.coordinates[1], message.rotation);
        GameHandler.gameHandler.changeTurn();
        GameHandler.gameHandler.setTurn();
        GameHandler.gameHandler.setAllTileDrag(true);
        GameHandler.gameHandler.cleanSolderedBoard();
    }

    private void drawTile(ResistileMessage message) //Draw a tile to your hands
    {
        GameHandler.gameHandler.Draw(message.tileID);
    }

    private void validMove(ResistileMessage message) //You End Turn
    {
        GameHandler.gameHandler.currentTile.GetComponent<Draggable>().enabled = false;
        GameHandler.gameHandler.currentTile.transform.FindChild("RotateButton").gameObject.SetActive(false);
        GameHandler.gameHandler.changeTurn();
        GameHandler.gameHandler.setTurn();
        GameHandler.gameHandler.setAllTileDrag(false);
        GameHandler.gameHandler.removeRotate(GameHandler.gameHandler.currentTile);
        GameHandler.gameHandler.cleanSolderedBoard();
        GameHandler.gameHandler.currentTile = null;
    }

    private void invalidMove(ResistileMessage message)
    {
        GameHandler.gameHandler.alert.alert("Invalid Move, Please Select a Valid Placement", 2.0f, true);
        if (GameHandler.gameHandler.currentTile.GetComponent<TileData>().type.Contains("Resistor"))
        {
            GameHandler.gameHandler.currentTile.transform.SetParent(GameHandler.gameHandler.resHand.transform, false);
        }
        else if (GameHandler.gameHandler.currentTile.GetComponent<TileData>().type.Contains("Wire"))
        {
            GameHandler.gameHandler.currentTile.transform.SetParent(GameHandler.gameHandler.wireHand.transform, false);
        }
        if(GameHandler.gameHandler.solderTile != null)
        {
            GameHandler.gameHandler.solderTile.transform.SetParent(GameHandler.gameHandler.resHand.transform, false);
        }
        GameHandler.gameHandler.solder = null;
        GameHandler.gameHandler.currentTile= null;
        GameHandler.gameHandler.setAllTileDrag(true);
    }

    private void gameOver(ResistileMessage messageFromThread)
    {
        GameHandler.gameHandler.guessScorePanel.SetActive(true);
        GameHandler.gameHandler.endTurn.interactable = false;
        GameHandler.gameHandler.setAllTileDrag(false);
        GameHandler.gameHandler.waitingForOpponent.SetActive(false);
    }

    private void gameResults(ResistileMessage message)
    {
        int pScore = 0, s1Score = 0, s2Score = 0, gScore = 0, tScore = 0;
        if ((bool)message.messageArray[0]) pScore = 2;
        if ((bool)message.messageArray[1]) s1Score = 1;
        if ((bool)message.messageArray[2]) s2Score = 1;
        if ((bool)message.messageArray[3]) gScore = 1;
        tScore = pScore + s1Score + s2Score + gScore;
        GameHandler.gameHandler.gameOver(message.win, pScore, s1Score, s2Score, gScore, tScore);
    }

    private void replay(ResistileMessage message)
    {
        if(message.replay == true)
        {
            SceneManager.LoadScene("Board");
        }
        else
        {
            GameHandler.gameHandler.noPlayAgain();
        }
    }

    private void opponentQuit(ResistileMessage message)
    {
        GameHandler.gameHandler.alert.alert("Opponent Left The Game, Returning To Main Menu", 4.0f, true);
        StartCoroutine(backToMainMenu());
    }

    private IEnumerator backToMainMenu()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("MainMenu");
    }

    //SEND MESSAGES TO SERVER
    public void gameLoaded()
    {
        NetworkManager.networkManager.sendMessage(new ResistileMessage(0, ResistileMessageTypes.gameLoaded, ""));
    }

    public void quitGame()
    {
        NetworkManager.networkManager.sendMessage(new ResistileMessage(GameHandler.gameHandler.gameID, ResistileMessageTypes.quitGame, ""));
        SceneManager.LoadScene("MainMenu");
    }

    public void endTurn()
    {
        ResistileMessage message = new ResistileMessage(GameHandler.gameHandler.gameID, ResistileMessageTypes.endTurn, "");
        if(GameHandler.gameHandler.solderTile != null)  //If solder was placed
        {
            TileData solderTile = GameHandler.gameHandler.solderTile.GetComponent<TileData>();
            message.solderId = solderTile.tileID;
            message.message = ResistileServer.GameTileTypes.solder;
        }
        else
        {
            message.solderId = 0;
        }
        message.tileID = GameHandler.gameHandler.currentTile.GetComponent<TileData>().tileID;
        message.rotation = GameHandler.gameHandler.currentTile.GetComponent<TileData>().rotation;
        GameObject parent = GameHandler.gameHandler.currentTile.transform.parent.gameObject;
        message.coordinates = new ArrayList(BoardHandler.CoordinatesOf(parent));
        NetworkManager.networkManager.sendMessage(message);
        GameHandler.gameHandler.currentTile.GetComponent<Draggable>().enabled = false;
    }

    public void guessResistance()
    {
        GameHandler.gameHandler.submitResButton.GetComponent<Button>().interactable = false;
        GameHandler.gameHandler.playerResGuess.GetComponent<InputField>().interactable = false;
        ResistileMessage message = new ResistileMessage(0, ResistileMessageTypes.guessResistance, "");
        message.guess = float.Parse(GameHandler.gameHandler.playerResGuess.text);
        NetworkManager.networkManager.sendMessage(message);
        
    }

    public void replay()
    {
        ResistileMessage message = new ResistileMessage(0, ResistileMessageTypes.replay, "");
        message.replay = true;
        NetworkManager.networkManager.sendMessage(message);
        GameHandler.gameHandler.playAgain.GetComponent<Button>().interactable = false;
        GameHandler.gameHandler.alert.alert("Waiting for Opponent to Accept", 3.0f, false);
    }

    public void noReplay()
    {
        ResistileMessage message = new ResistileMessage(0, ResistileMessageTypes.replay, "");
        message.replay = false;
        NetworkManager.networkManager.sendMessage(message);
        SceneManager.LoadScene("MainMenu");
    }

    private bool ping()
    {
        return NetworkManager.networkManager.sendMessage(new ResistileMessage(0, ResistileMessageTypes.ping, ""));
    }
}
