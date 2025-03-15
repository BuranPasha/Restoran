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

    // Para eklemek için kullanýlan fonksiyon
    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    // Para çýkarmak için kullanýlan fonksiyon
    public void RemoveMoney(int amount)
    {
        if (amount > money)
        {
            money = 0;  // Eðer yeterli para yoksa, tüm parayý sýfýrlýyoruz
        }
        else
        {
            money -= amount;
        }
        UpdateMoneyUI();
    }

    // Para miktarýný UI'ye yansýtma
    private void UpdateMoneyUI()
    {
        moneyText.text = "Money: " + money.ToString();
    }
}
