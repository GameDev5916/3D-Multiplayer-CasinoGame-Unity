using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

namespace RouletteCasino
{
    public class BallController : MonoBehaviour
    {
        [SerializeField]
        private GameObject wheel;
        [SerializeField]
        private AnimationClip[] ballAnimations;
        private bool isBallSpin, isWheelSpin;

        private int winNum, waitSec;
        private float ballSpeed, wheelSpeed;


        private void OnEnable()
        {
            GetComponent<Animator>().Play(ballAnimations[UnityEngine.Random.Range(0, ballAnimations.Length)].name);
            transform.GetChild(0).GetComponent<Collider2D>().enabled = false;
            ballSpeed = 500;
            wheelSpeed = 100;
            winNum = GameManager_Roulette.instance.winNum;
            waitSec = GameManager_Roulette.instance.waitSec;
            isBallSpin = true;
            isWheelSpin = true;
            StartCoroutine(StopBall());
        }

        void Update()
        {
            if (isBallSpin)
            {
                ballSpeed -= Time.deltaTime * 20;
                transform.Rotate(Vector3.back * Time.deltaTime * ballSpeed);
            }

            if (isWheelSpin)
            {
                if (!isBallSpin) wheelSpeed -= Time.deltaTime * 20f;
                else wheelSpeed -= Time.deltaTime * 2f;
                wheel.transform.Rotate(Vector3.forward * Time.deltaTime * wheelSpeed);
            }
        }

        IEnumerator StopBall()
        {
            yield return new WaitForSeconds(waitSec);
            transform.GetChild(0).GetComponent<Collider2D>().enabled = true;
            Invoke("ResetWheel", 5f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "number" && collision.name == winNum.ToString())
            {
                isBallSpin = false;
                transform.GetChild(0).GetComponent<Collider2D>().enabled = false;
                transform.rotation = collision.gameObject.transform.rotation;

                print("Win Num == " + winNum);
                print("Collision Num == " + collision.gameObject.name);

                GameManager_Roulette.instance.winningNoText.text = winNum.ToString();
                GameManager_Roulette.instance.winningAmtText.text = "$0";
            }
        }

        private void ResetWheel()
        {
            isWheelSpin = false;
            gameObject.SetActive(false);
            GameManager_Roulette.instance.Clear(1); // 0 for clear button and 1 for reset
            GameManager_Roulette.instance.spinBtn.GetComponent<Button>().interactable = true;
            GameManager_Roulette.instance.homeBtn.GetComponent<Button>().interactable = true;
            GameManager_Roulette.instance.isgameStart = false;
            GameManager_Roulette.instance.name = "fes";
        }
    }
}
