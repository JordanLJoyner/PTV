using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VotingScript : MonoBehaviour{
    [SerializeField] private GameObject m_VotingBlockPrefab;
    [SerializeField] private GameObject m_VotingGrid;
    List<VotingStruct> mVotingTri = new List<VotingStruct>();

    private class VotingStruct {
        public VotingBlock mVotingBlock;
        public string mName;
        public int mCurrentVotes;
        public VotingStruct(VotingBlock block, string name) {
            mName = name;
            mVotingBlock = block;
            mCurrentVotes = 0;
        }
    }

    private void Reset() {
        for(int i=0; i < mVotingTri.Count; i++) {
            GameObject.Destroy(mVotingTri[i].mVotingBlock.gameObject);
        }
        mVotingTri.Clear(); 
    }

    public void StartVote(List<string> voteOptions) {
        Reset();
        for (int i = 0; i < voteOptions.Count; i++) {
            string voteOption = voteOptions[i];
            GameObject newVoteBlock = Instantiate(m_VotingBlockPrefab, m_VotingGrid.transform);
            VotingBlock block = newVoteBlock.GetComponent<VotingBlock>();
            block.Init(voteOption, i);
            mVotingTri.Add(new VotingStruct(block, voteOption));
        }
        StartCoroutine(RESTApiTest.StartDraftOnServer(voteOptions));
    }

    public void RegisterVote(int index) {
        if (mVotingTri.Count == 0) {
            return;
        }
        mVotingTri[index].mCurrentVotes++;
        mVotingTri[index].mVotingBlock.UpdateVoteText(mVotingTri[index].mCurrentVotes);
    }

    public void RegisterVote(string itemName) {
        for (int i = 0; i < mVotingTri.Count; i++) {
            if (mVotingTri[i].mName.Equals(itemName)) {
                RegisterVote(i);
                break;
            }
        }
    }

    public string GetVoteWinner() {
        if (mVotingTri.Count == 0) {
            return "Voting didn't start";
        }
        int currentHighestVotes = 0;
        List<int> indexesWithHighestVotes = new List<int>();
        for (int i = 0; i < mVotingTri.Count; i++) {
            int currentVotes = mVotingTri[i].mCurrentVotes;
            if (currentVotes > currentHighestVotes) {
                indexesWithHighestVotes.Clear();
            }
            if (currentVotes >= currentHighestVotes) {
                currentHighestVotes = currentVotes;
                indexesWithHighestVotes.Add(i);
            }
        }
        int chosenIndex = indexesWithHighestVotes[UnityEngine.Random.Range(0, indexesWithHighestVotes.Count)];
        return mVotingTri[chosenIndex].mName;
    }
}
