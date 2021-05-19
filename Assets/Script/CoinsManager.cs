using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    public void AddCoins(int amount)
    {
        numberOfCoins += amount;

        DOTween.To(() => int.Parse(coinAmountDisplay.text), x => coinAmountDisplay.text = x.ToString(), numberOfCoins, 0.05f * amount);

        coinDisplayObject.transform.DOKill();
        coinDisplayObject.localScale = Vector3.one;
        coinDisplayObject.transform.DOPunchScale(Vector3.one * 0.2f * Mathf.Clamp(amount, 1f, 4f), 0.25f, 10, 0.25f);
    }

}
