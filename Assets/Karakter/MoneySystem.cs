using UnityEngine;
using TMPro; // UI için gerekli

public class MoneySystem : MonoBehaviour
{
    public static MoneySystem instance;
    public int money = 0;
    public TextMeshProUGUI moneyText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        moneyText.text = "Para: " + money.ToString();
    }
}
