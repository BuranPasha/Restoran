using UnityEngine;
using TMPro;
using Photon.Pun;

public class BankingSystem : MonoBehaviourPunCallbacks, IPunObservable
{
    public static BankingSystem instance;
    public int currentDebt = 0;  // Þu anki borç
    public int loanAmount = 0;   // Çekilen kredi miktarý
    public int moneyToBorrow = 0; // Kredi limitleri (5000, 10000, 50000)
    public TextMeshProUGUI loanText;  // Krediyi göstermek için UI
    public TextMeshProUGUI debtText;  // Borcu göstermek için UI
    public TextMeshProUGUI balanceText;  // Mevcut para miktarý
    public TextMeshProUGUI interestRateText;  // Faiz oranýný göstermek için UI
    private float interestRate = 0.02f;  // Faiz oraný (günlük %2)
    private float daysPassed = 0; // Gün sayýsýný tutan deðiþken

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        UpdateBankingUI();
    }

    private void Update()
    {
        // Her gün için faiz uygulama (gün sayýsý arttýkça)
        daysPassed += Time.deltaTime;  // Zamaný takip et (günler hesaplanacak)

        // Eðer 1 gün geçerse, faiz eklenir
        if (daysPassed >= 86400f) // 86400 saniye = 1 gün
        {
            ApplyInterest();
            daysPassed = 0;  // Günü sýfýrla
        }
    }

    // Kredi çekme fonksiyonu
    public void TakeLoan(int amount)
    {
        if (currentDebt > 0)
        {
            // Eðer borç varsa yeni kredi alýnamaz
            Debug.Log("You cannot take a new loan until you pay off the current debt.");
            return;
        }

        if (moneyToBorrow == 0)
        {
            moneyToBorrow = amount;
            loanAmount = moneyToBorrow;
            currentDebt += loanAmount;
            MoneySystem.instance.AddMoney(loanAmount);  // Krediyi oyuncuya ekle
            UpdateBankingUI();
        }
    }

    // Kredi ödeme fonksiyonu
    // Kredi ödeme fonksiyonu
    public void PayDebt(int paymentAmount)
    {
        int playerMoney = MoneySystem.instance.money; // Oyuncunun mevcut parasý

        if (currentDebt <= 0)
        {
            Debug.Log("There is no debt to pay.");
            return;
        }

        if (playerMoney <= 0)
        {
            Debug.Log("Not enough money to pay the debt.");
            return;
        }

        // Oyuncunun tüm parasýný alarak ödeme yapmasýný saðla
        if (paymentAmount > playerMoney)
        {
            paymentAmount = playerMoney; // En fazla oyuncunun sahip olduðu kadar ödeyebilir
        }

        // Eðer ödeme miktarý borçtan büyükse, sadece borç kadarýný öde
        if (paymentAmount > currentDebt)
        {
            paymentAmount = currentDebt; // Fazla ödeme engellenir
        }

        // Ödemeyi gerçekleþtir
        currentDebt -= paymentAmount;
        MoneySystem.instance.RemoveMoney(paymentAmount); // Oyuncunun parasýndan düþ

        // Eðer borç sýfýrlanýrsa, kredi sýfýrlanýr
        if (currentDebt == 0)
        {
            loanAmount = 0;
            moneyToBorrow = 0;
        }

        UpdateBankingUI(); // UI'yi güncelle
    }


    // Faiz ekleme fonksiyonu (borç ekleme)
    // BankingSystem.cs
    public void ApplyInterest()
    {
        if (currentDebt > 0)
        {
            // Günlük faiz ekleniyor (%2)
            currentDebt += Mathf.FloorToInt(currentDebt * interestRate);
            UpdateBankingUI();  // UI güncelleniyor
        }
    }


    // Banka bilgilerini UI'ye yansýtma
    public void UpdateBankingUI()
    {
        loanText.text = "Loan: " + loanAmount.ToString();
        debtText.text = "Debt: " + currentDebt.ToString();
        balanceText.text = "Money: " + MoneySystem.instance.money.ToString();
        interestRateText.text = "Interest Rate: " + (interestRate * 100).ToString() + "%";  // Faiz oranýný göster
    }

    // Photon senkronizasyonu
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Master Client veri gönderiyor
        {
            stream.SendNext(currentDebt);
            stream.SendNext(loanAmount);
            stream.SendNext(moneyToBorrow);
        }
        else // Diðer oyuncular veriyi alýyor
        {
            currentDebt = (int)stream.ReceiveNext();
            loanAmount = (int)stream.ReceiveNext();
            moneyToBorrow = (int)stream.ReceiveNext();
        }
    }
}
