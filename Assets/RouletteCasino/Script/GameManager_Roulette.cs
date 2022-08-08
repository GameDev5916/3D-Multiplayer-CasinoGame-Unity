using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Roulette : MonoBehaviour
{
    public static GameManager_Roulette instance;

    public Text winningNoText, winningAmtText, totalbalanceText, betAmountText;
    [HideInInspector]
    public int winningAmt, totalBalance, betamount;

    public GameObject ball, spinBtn, homeBtn, clearBtn, UndoBtn;
    public Stack<GameObject> chips = new Stack<GameObject>();

    public bool isgameStart = false;
    public int winNum;
    public int waitSec;

    private void Awake()
    {
        instance = this;
        ball.SetActive(false);
    }

    private void Start()
    {
        winNum = UnityEngine.Random.Range(0, 37);
        waitSec = 4;
        totalBalance =(int) Constants.CHIPS;
        totalbalanceText.text = "$" + totalBalance;
    }

    public void SpinBall()
    {
        if (isgameStart) return;

        if (chips.Count > 0)
        {
            isgameStart = true;
            spinBtn.GetComponent<Button>().interactable = false;
            homeBtn.GetComponent<Button>().interactable = false;
            clearBtn.SetActive(false);
            UndoBtn.SetActive(false);
            ball.SetActive(true);
        }
    }

    public void Clear(int index)
    {
        foreach (GameObject g in chips)
            Destroy(g);
        chips.Clear();
        clearBtn.SetActive(false);
        UndoBtn.SetActive(false);
        if (index == 0)
        {
            totalBalance += betamount;
            betamount = 0;
            totalbalanceText.text = "$" + totalBalance;
        }

        winningAmtText.gameObject.SetActive(false);
        betAmountText.gameObject.SetActive(false);
        winningAmt = 0;
        betamount = 0;
        winningNoText.text = "";
        Constants.CHIPS = totalBalance;
    }

    public void Undo()
    {
        if (chips.Count > 0)
        {
            GameObject _chip = chips.Peek();
            int _chipAmt = int.Parse(_chip.GetComponentInChildren<Text>().text);
            totalBalance += _chipAmt;
            totalbalanceText.text = "$" + totalBalance;
            betamount -= _chipAmt;
            betAmountText.text = "$" + betamount;
            Destroy(_chip);
            chips.Pop();
            if (chips.Count <= 0)
            {
                betAmountText.gameObject.SetActive(false);
                clearBtn.SetActive(false);
                UndoBtn.SetActive(false);
            }
        }
    }

    public void SetWinningAmt()
    {
        if (winningAmt > 0)
        {
            winningAmtText.text = "$" + winningAmt;
            winningAmtText.gameObject.SetActive(true);
            totalBalance += winningAmt;
            totalbalanceText.text = "$" + totalBalance;
            Constants.CHIPS = totalBalance;
            // win
        }
        else
        {
            // lose
        }
    }

    public void CountBetAmount(int amt)
    {
        totalBalance -= amt;
        betamount += amt;
        totalbalanceText.text = "$" + totalBalance;
        betAmountText.text = "$" + betamount;
        betAmountText.gameObject.SetActive(true);

        if (betamount > 0)
        {
            //clearBtn.SetActive(true);
            //UndoBtn.SetActive(true);
        }
    }
}
