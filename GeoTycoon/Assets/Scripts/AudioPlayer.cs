using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer instance { get; private set;}
    public static float BGMVolume = 1.0f;
    public AudioSource SFX_SRC;
    public AudioSource BGM_SRC;
    public AudioClip button_click, 
    answer_quiz_right, answer_quiz_wrong,
    buy, car_park, money_pay, money_collect, draw_card, jail, dice;
    public AudioClip bgm1, bgm2, bgm3;

    private void Awake() 
    {
        instance = this;
    }
    private void Start() {
        SetBGMVolume(BGMVolume);
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene != null)
        {
            if (currentScene.name == "StartMenu")
            {
                BGM_SRC.clip = bgm1;
                BGM_SRC.Play();
            }
            if (currentScene.name == "MainMenu")
            {
                BGM_SRC.clip = bgm1;
                BGM_SRC.Play();
            }
            else if (currentScene.name == "MainGame")
            {
                BGM_SRC.clip = bgm2;
                BGM_SRC.Play();
            }
        }
    }
    public void SetBGMVolume(float volume)
    {
        BGM_SRC.volume = volume;
        BGMVolume = volume;
    }
    public void GameOverBGM()
    {
        BGM_SRC.clip = bgm3;
        BGM_SRC.Play();
    }

    public void ClickButtonSound()
    {
        SFX_SRC.clip = button_click;
        SFX_SRC.Play();
    }
    public void QuizCorrect()
    {
        SFX_SRC.clip = answer_quiz_right;
        SFX_SRC.Play();
    }
    public void QuizWrong()
    {
        SFX_SRC.clip = answer_quiz_wrong;
        SFX_SRC.Play();
    }
    public void Buy()
    {
        SFX_SRC.clip = buy;
        SFX_SRC.Play();
    }
    public void CarPark()
    {
        SFX_SRC.clip = car_park;
        SFX_SRC.Play();
    }
    public void Pay()
    {
        SFX_SRC.clip = money_pay;
        SFX_SRC.Play();
    }
    public void Collect()
    {
        SFX_SRC.clip = money_collect;
        SFX_SRC.Play();
    }

    public void DrawCard()
    {
        SFX_SRC.clip = draw_card;
        SFX_SRC.Play();
    }
    public void Jail()
    {
        SFX_SRC.clip = jail;
        SFX_SRC.Play();
    }
    public void DiceDrop()
    {
        SFX_SRC.clip = dice;
        SFX_SRC.Play();
    }
}
