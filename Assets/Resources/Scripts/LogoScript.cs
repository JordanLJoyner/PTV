using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class LogoScript : MonoBehaviour {
    public AudioSource SFXPlayer;
    public AudioClip CursorSFX;
    public AudioClip SaveSFX;
    public TextMeshProUGUI LogoText;
    private Action completeReference;

    public void PlayLogo(Action onLogoComplete) {
        LogoText.enabled = true;
        completeReference = onLogoComplete;
        StartCoroutine(DoLogo());
    }

    IEnumerator DoLogo() {
        SFXPlayer.clip = CursorSFX;
        SFXPlayer.time = 0.5f;
        SFXPlayer.Play();
        yield return new WaitForSeconds(0.5f);
        SFXPlayer.time = 0.5f;
        SFXPlayer.Play();
        yield return new WaitForSeconds(0.5f);
        SFXPlayer.time = 0.5f;
        SFXPlayer.Play();
        yield return new WaitForSeconds(0.5f);
        SFXPlayer.clip = SaveSFX;
        SFXPlayer.Play();
        yield return new WaitForSeconds(1.25f);
        if (completeReference != null) {
            completeReference();
        }
        LogoText.enabled = false;
    }
}
