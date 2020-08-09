using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DisplayAvailableTagsScript : MonoBehaviour
{
    public GridLayoutGroup AllTagsGrid;
    public GameObject PlusMinusBlockPrefab;

    Action<string> mOnPlusClicked;
    private void Start() {
        //Load all the tags and display them as PlusMisBlocks
    }

    public void Setup(Action<string> onPlusClicked) {
        mOnPlusClicked = onPlusClicked;
    }

    public void _OnPlusTagClicked() {

    }
}
