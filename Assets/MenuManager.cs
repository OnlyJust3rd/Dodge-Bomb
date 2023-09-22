using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public string[] sceneName;
    public GameObject bombEffect;
    public GameObject[] biggyBomb;
    public GameObject[] stuffThatGonnaShowUp;

    private scrpt_AudioManager audioManager;

    private void Start()
    {
        Time.timeScale = 1;
        audioManager = FindObjectOfType<scrpt_AudioManager>();
        foreach (GameObject stuff in biggyBomb) stuff.SetActive(true);
        foreach (GameObject stuff in stuffThatGonnaShowUp) stuff.SetActive(false);
        StartCoroutine(boomboom());
    }

    private IEnumerator boomboom()
    {
        audioManager.Play("ticking");
        yield return new WaitForSeconds(1);
        audioManager.Stop("ticking");
        audioManager.Play("menu bgm");
        audioManager.Play("boom");
        foreach (GameObject stuff in biggyBomb) stuff.SetActive(false);
        GameObject newBombEffect = Instantiate(bombEffect, new Vector2(-2, 9), Quaternion.identity) as GameObject;
        Destroy(newBombEffect, 1);
        foreach (GameObject stuff in stuffThatGonnaShowUp) stuff.SetActive(true); 
    }

    public void StartGame()
    {
        audioManager.Play("button");
        audioManager.Stop("menu bgm");
        int playMap = Random.Range(0, sceneName.Length);
        if (playMap == 0 || playMap == 2) audioManager.Play("cave bgm");
        else if (playMap == 1) audioManager.Play("grass bgm");
        SceneManager.LoadScene(sceneName[playMap]);
    }

    public void Quit()
    {
        audioManager.Play("button");
        Application.Quit();
    }
}
