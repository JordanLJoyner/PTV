using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class VotingBlock : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI m_LabelText;
    [SerializeField] private TextMeshProUGUI m_VoteText;
    public void Init(string label, int index) {
        m_LabelText.text = "Option " + index.ToString() + ": " + label;
        UpdateVoteText(0);
    }
    public void UpdateVoteText(int newValue) {
        m_VoteText.text = "- " + newValue.ToString() + " Votes";
    }
}
