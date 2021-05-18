using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinsManager : MonoBehaviour
{
    public static CoinsManager instance;
    int numberOfCoins;
    [SerializeField]
    TextMeshProUGUI coinAmountDisplay;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            AddCoins(1);
        //coinAmountDisplay.text = numberOfCoins.ToString();
    }

    public void AddCoins(int amount)
    {
        numberOfCoins += amount;

        iTween.ValueTo(gameObject, iTween.Hash(
        "from", int.Parse(coinAmountDisplay.text),
        "to", numberOfCoins,
        "time", 0.05f * amount,
        "onupdatetarget", gameObject,
        "onupdate", "IncreaseCoinUpdateCallBack",
        "easetype", iTween.EaseType.linear
        )
        );
    }


    void IncreaseCoinUpdateCallBack(int newValue)
    {
        coinAmountDisplay.text = newValue.ToString();
        // Debug.Log(exampleInt);
    }
}
