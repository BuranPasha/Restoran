using UnityEngine;
using TMPro;
using Photon.Pun;

public class DayNightSystem : MonoBehaviourPunCallbacks, IPunObservable
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public Light sunLight;
    public float timeSpeed = 60f;  // Zaman hýzý, saniyede 60 dakika
    private float timeElapsed = 0f; // Zamanýn ilerlemesi için sayaç

    private int hour = 7;
    private int minute = 0;
    private int day = 1;

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient) // Sadece Master Client zamaný ilerletir
        {
            UpdateTime();
        }
        UpdateLighting(); // Herkes ýþýðý günceller
    }

    private void UpdateTime()
    {
        // Sabit zaman hýzýna göre dakika ekle
        timeElapsed += Time.deltaTime * timeSpeed;
        while (timeElapsed >= 60f)  // 60 saniyelik bir süre geçtiðinde
        {
            timeElapsed -= 60f;  // Zaman sayaçýný sýfýrla
            minute++;  // Dakikayý arttýr

            if (minute >= 60)
            {
                minute = 0;
                hour++;  // Saat arttýr

                if (hour == 24) // 11:59 PM’den sonra gün deðiþimi
                {
                    hour = 0;
                    day++; // Gün arttýr

                    // Gün deðiþtiðinde faiz uygulamasý yapýlýr
                    BankingSystem.instance.ApplyInterest();  // Burada faiz uygulama fonksiyonu çaðrýlýr
                }
            }
        }

        // Saat ve gün bilgilerini UI'ye yansýtma
        timeText.text = $"{hour:D2}:{minute:D2} {(hour < 12 ? "AM" : "PM")}";
        dayText.text = $"Day {day}";
    }

    void UpdateLighting()
    {
        // Saat aralýklarýný tanýmlayalým
        float sunsetStart = 18f; // 18:00'de gün batýmý baþlar
        float nightStart = 20f;  // 20:00'de tam gece baþlar
        float sunriseStart = 4f; // 04:00'te gün doðumu baþlar
        float dayStart = 7f;     // 07:00'de tamamen gündüz baþlar

        // Geçiþ için normalleþtirilmiþ zaman deðeri
        float normalizedTime = 0f;

        if (hour >= sunsetStart && hour < nightStart)
        {
            // 18:00 - 20:00 Arasý (Gün Batýmý)
            normalizedTime = Mathf.InverseLerp(sunsetStart, nightStart, hour + minute / 60f);
        }
        else if (hour >= nightStart || hour < sunriseStart)
        {
            // 20:00 - 04:00 Arasý (Tam Gece, En Karanlýk Zaman)
            normalizedTime = 1f; // Tamamen karanlýk
        }
        else if (hour >= sunriseStart && hour < dayStart)
        {
            // 04:00 - 07:00 Arasý (Gün Doðumu)
            normalizedTime = Mathf.InverseLerp(sunriseStart, dayStart, hour + minute / 60f);
            normalizedTime = 1 - normalizedTime; // Gün doðumu sýrasýnda karanlýktan aydýnlýða geçiþ
        }
        else
        {
            // 07:00 - 18:00 Arasý (Tamamen Gündüz)
            normalizedTime = 0f; // Tamamen aydýnlýk
        }

        // Gece ýþýðýný daha karanlýk yapmak için minIntensity deðerini düþürelim
        float minIntensity = 0.005f; // Gece için daha düþük ýþýk
        float maxIntensity = 1.2f;   // Gündüz için yüksek ýþýk
        sunLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, 1 - normalizedTime);

        // Gün ýþýðýnýn rengi (gün batýmý ve doðumu için kýrmýzýmsý, gece için morumsu)
        sunLight.color = Color.Lerp(new Color(1f, 0.8f, 0.6f), new Color(0.1f, 0.1f, 0.3f), normalizedTime);

        // Ortam ýþýðýný daha karanlýk yapalým
        RenderSettings.ambientLight = Color.Lerp(Color.white, new Color(0.05f, 0.05f, 0.1f), normalizedTime);
    }

    void ApplyDebtInterest()
    {
        if (BankingSystem.instance.currentDebt > 0)
        {
            BankingSystem.instance.currentDebt += Mathf.FloorToInt(BankingSystem.instance.currentDebt * 0.02f);  // %2 faiz
            BankingSystem.instance.UpdateBankingUI();  // UI güncelleme
        }
    }


    public void TrySleep()
    {
        if (hour >= 22 && PhotonNetwork.IsMasterClient)
        {
            hour = 7;
            minute = 0;
            day++;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Master Client veri gönderiyor
        {
            stream.SendNext(hour);
            stream.SendNext(minute);
            stream.SendNext(day);
        }
        else // Diðer oyuncular veriyi alýyor
        {
            hour = (int)stream.ReceiveNext();
            minute = (int)stream.ReceiveNext();
            day = (int)stream.ReceiveNext();
        }
    }
}
