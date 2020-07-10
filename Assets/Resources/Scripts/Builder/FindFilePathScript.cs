using UnityEditor;
using UnityEngine;
using TMPro;

public class FindFilePathScript : MonoBehaviour {
    public delegate void OnFilePathFound(string chosenFilePath);
    public event OnFilePathFound OnFilePathFoundEvent;
    public TextMeshProUGUI FilePathText;
    private string lastKnownPath = "";

    public void SetDefaultPath(string defaultPath) {
        lastKnownPath = defaultPath;
    }

    public void _OnFindFilePathClicked() {
        string path = EditorUtility.OpenFolderPanel("", lastKnownPath, "");
        if (path != "" && OnFilePathFoundEvent != null) {
            OnFilePathFoundEvent(path);
            lastKnownPath = path;
        }
    }

}
