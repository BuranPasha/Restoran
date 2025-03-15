using UnityEngine;
using TMPro;
using Photon.Pun;

public class DayNightSystem : MonoBehaviourPunCallbacks, IPunObservable
{
    public static DayNightSystem Instance;

    [Header("UI References")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;

    [Header("Lighting Settings")]
    public Light sunLight;
    public float timeSpeed = 60f;

    // Network-synchronized time data
    private int currentHour = 7;
    private int currentMinute = 0;
    private int currentDay = 1;
    private float timeAccumulator = 0f;

    // Lighting control parameters
    private const float sunsetStart = 18f;
    private const float nightStart = 20f;
    private const float sunriseStart = 4f;
    private const float dayStart = 7f;

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

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateMasterTime();
        }
        UpdateLighting();
        UpdateTimeDisplay();
    }

    void UpdateMasterTime()
    {
        timeAccumulator += Time.deltaTime * timeSpeed;

        while (timeAccumulator >= 60f)
        {
            timeAccumulator -= 60f;
            currentMinute++;

            if (currentMinute >= 60)
            {
                currentMinute = 0;
                currentHour++;

                if (currentHour >= 24)
                {
                    currentHour = 0;
                    currentDay++;
                    BankingSystem.Instance.ApplyInterest();
                }
            }
        }
    }

    void UpdateTimeDisplay()
    {
        string period = currentHour < 12 ? "AM" : "PM";
        int displayHour = currentHour % 12;
        displayHour = displayHour == 0 ? 12 : displayHour;

        timeText.text = $"{displayHour:D2}:{currentMinute:D2} {period}";
        dayText.text = $"Day {currentDay}";
    }

    void UpdateLighting()
    {
        float currentTime = currentHour + currentMinute / 60f;
        float normalizedTime = CalculateNormalizedTime(currentTime);

        UpdateSunParameters(normalizedTime);
        UpdateAmbientLight(normalizedTime);
    }

    float CalculateNormalizedTime(float currentTime)
    {
        if (currentTime >= sunsetStart && currentTime < nightStart)
        {
            return Mathf.InverseLerp(sunsetStart, nightStart, currentTime);
        }
        else if (currentTime >= nightStart || currentTime < sunriseStart)
        {
            return 1f;
        }
        else if (currentTime >= sunriseStart && currentTime < dayStart)
        {
            return 1 - Mathf.InverseLerp(sunriseStart, dayStart, currentTime);
        }
        return 0f;
    }

    void UpdateSunParameters(float normalizedTime)
    {
        const float minIntensity = 0.005f;
        const float maxIntensity = 1.2f;

        sunLight.intensity = Mathf.Lerp(maxIntensity, minIntensity, normalizedTime);
        sunLight.color = Color.Lerp(
            new Color(1f, 0.8f, 0.6f),
            new Color(0.1f, 0.1f, 0.3f),
            normalizedTime
        );
    }

    void UpdateAmbientLight(float normalizedTime)
    {
        RenderSettings.ambientLight = Color.Lerp(
            Color.white,
            new Color(0.05f, 0.05f, 0.1f),
            normalizedTime
        );
    }

    public void TrySleep()
    {
        if (currentHour >= 22 && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("AdvanceToMorningRPC", RpcTarget.AllViaServer);
        }
    }

    [PunRPC]
    void AdvanceToMorningRPC()
    {
        currentHour = 7;
        currentMinute = 0;
        currentDay++;
        BankingSystem.Instance.ApplyInterest();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHour);
            stream.SendNext(currentMinute);
            stream.SendNext(currentDay);
        }
        else
        {
            currentHour = (int)stream.ReceiveNext();
            currentMinute = (int)stream.ReceiveNext();
            currentDay = (int)stream.ReceiveNext();
        }
    }

    // Public accessors
    public int CurrentHour => currentHour;
    public int CurrentMinute => currentMinute;
    public int CurrentDay => currentDay;
}