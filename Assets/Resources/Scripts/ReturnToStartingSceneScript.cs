using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToStartingSceneScript : MonoBehaviour {
    public void _OnBackToStartingSceneClicked() {
        SceneManager.LoadScene("StartingScene");
    }
}
