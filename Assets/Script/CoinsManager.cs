using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinsManager : MonoBehaviour
{
    public static CoinsManager instance;
    int numberOfCoins;
    [SerializeField]
    TextMeshProUGUI coinAmountDisplay;
    RectTransform coinDisplayObject;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        coinDisplayObject = coinAmountDisplay.transform.parent.GetComponent<RectTransform>();
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
        StartCoroutine(ShakeCoinDisplay(amount));
    }


    void IncreaseCoinUpdateCallBack(int newValue)
    {
        coinAmountDisplay.text = newValue.ToString();
        // Debug.Log(exampleInt);
    }

    IEnumerator ShakeCoinDisplay(int amount)
    {
        iTween.Stop(coinDisplayObject.gameObject, false);
        coinDisplayObject.localScale = Vector3.one;
        yield return new WaitForSeconds(0.1f);
        iTween.PunchScale(coinDisplayObject.gameObject, Vector3.one * 0.2f * Mathf.Clamp(amount, 1f, 4f), 0.25f);
        yield break;
    }
}
