using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlusMinusBlock : MonoBehaviour {
    public Button MinusButton;
    public Button PlusButton;
    public TextMeshProUGUI text;

    private Action<string> mPostiveAction;
    private Action<string> mNegativeAction;

    public void Setup(Action<string> plusAction, Action<string> minusAction, string textValue) {
        PlusButton.gameObject.SetActive(plusAction != null);
        MinusButton.gameObject.SetActive(minusAction != null);
        text.text = textValue;
        mPostiveAction = plusAction;
        mNegativeAction = minusAction;
    }

    public void _OnPlusClick() {
        if(mPostiveAction != null) {
            mPostiveAction(text.text);
        }
    }

    public void _OnMinusClick() {
        if (mNegativeAction != null) {
            mNegativeAction(text.text);
        }
    }
}
