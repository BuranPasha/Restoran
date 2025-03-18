using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Globalization;

public class BankingSystem : MonoBehaviourPunCallbacks, IPunObservable
{
    public static BankingSystem Instance;

    [Header("UI References")]
    public TMP_Text loanText;
    public TMP_Text debtText;
    public TMP_Text balanceText;
    public TMP_Text interestRateText;
    public TMP_Text moneyText; // UI'deki Money yazýsý


    // Networked financial data
    private int sharedMoney;
    private int currentDebt;
    private const float interestRate = 0.02f;
    private int activeLoanAmount;

    private PhotonView photonView;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            photonView = GetComponent<PhotonView>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeFinancials();
        }
        UpdateBankingUI();
    }

    void InitializeFinancials()
    {
        sharedMoney = 0;
        currentDebt = 0;
        activeLoanAmount = 0;
    }

    [PunRPC]
    void SyncFinancialsRPC(int money, int debt, int loan)
    {
        sharedMoney = money;
        currentDebt = debt;
        activeLoanAmount = loan;
        UpdateBankingUI();
    }

    public void AddFunds(int amount)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ModifyFundsRPC", RpcTarget.All, amount);
        }
    }

    [PunRPC]
    void ModifyFundsRPC(int amount)
    {
        sharedMoney += amount;
        UpdateBankingUI();
    }

    public void TakeLoan(int amount)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentDebt > 0)
            {
                Debug.Log("Clear existing debt first!");
                return;
            }

            photonView.RPC("ProcessLoanRPC", RpcTarget.All, amount);
        }
    }

    [PunRPC]
    void ProcessLoanRPC(int amount)
    {
        activeLoanAmount = amount;
        currentDebt = amount;
        sharedMoney += amount;
        UpdateBankingUI();
    }

    public void PayDebt(int amount)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int payment = Mathf.Min(amount, sharedMoney, currentDebt);

            photonView.RPC("ProcessPaymentRPC", RpcTarget.All, payment);
        }
    }

    [PunRPC]
    void ProcessPaymentRPC(int payment)
    {
        sharedMoney -= payment;
        currentDebt -= payment;

        if (currentDebt <= 0)
        {
            activeLoanAmount = 0;
        }

        UpdateBankingUI();
    }

    public void ApplyInterest()
    {
        if (PhotonNetwork.IsMasterClient && currentDebt > 0)
        {
            photonView.RPC("CalculateInterestRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void CalculateInterestRPC()
    {
        int interest = Mathf.FloorToInt(currentDebt * interestRate);
        currentDebt += interest;
        UpdateBankingUI();
    }

    void UpdateBankingUI()
    {
        CultureInfo usdCulture = new CultureInfo("en-US");

        balanceText.text = $"Shared Funds: {sharedMoney.ToString("C", usdCulture)}";
        debtText.text = $"Total Debt: {currentDebt.ToString("C", usdCulture)}";
        loanText.text = $"Active Loan: {activeLoanAmount.ToString("C", usdCulture)}";
        interestRateText.text = $"Interest Rate: {interestRate * 100}%";

        // Para UI'sini güncelle
        if (moneyText != null)
        {
            moneyText.text = $"Money: {sharedMoney}$";
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(sharedMoney);
            stream.SendNext(currentDebt);
            stream.SendNext(activeLoanAmount);
        }
        else
        {
            sharedMoney = (int)stream.ReceiveNext();
            currentDebt = (int)stream.ReceiveNext();
            activeLoanAmount = (int)stream.ReceiveNext();
            UpdateBankingUI();
        }
    }

    // Public accessors
    public int SharedMoney => sharedMoney;
    public int CurrentDebt => currentDebt;
    public int ActiveLoan => activeLoanAmount;
}