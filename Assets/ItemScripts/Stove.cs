using Photon.Pun;
using UnityEngine;

public class Stove : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isOn = false;  // Ocak a��k m� kapal� m�?
    public GameObject fireEffect;  // Ocak a��kken g�sterilecek ate� efekti
    public Light stoveLight;  // Ocak a��kken yanacak ���k (opsiyonel)
    public Pan pan;  // Tava referans�
    public float cookingTime = 10f;  // Pi�irme s�resi (saniye)
    public float burningTime = 15f;  // Yanma s�resi (saniye)
    private float currentCookingTime = 0f;  // Ge�en pi�irme s�resi
    private bool isCooking = false;  // Pi�irme i�lemi devam ediyor mu?
    public float interactionDistance = 3f;  // Etkile�im mesafesi

    void Update()
    {
        // PhotonView null ise i�lem yapma
        if (photonView == null) return;

        // Herhangi bir oyuncu oca�� a��p kapatabilir
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleStove();
        }

        // Ocak a��kken ve tavada et varsa pi�irme i�lemi ba�lat
        if (isOn && pan != null && pan.itemOnPan != null)
        {
            CookItem();
        }
    }

    void ToggleStove()
    {
        isOn = !isOn;
        photonView.RPC("RPC_ToggleStove", RpcTarget.All, isOn);
    }

    [PunRPC]
    void RPC_ToggleStove(bool stoveState)
    {
        isOn = stoveState;
        if (fireEffect != null) fireEffect.SetActive(isOn);  // Ate� efekti a��k/kapal� durumuna g�re g�ncellenir
        if (stoveLight != null) stoveLight.enabled = isOn;  // I��k a��k/kapal� durumuna g�re g�ncellenir
    }

    void CookItem()
    {
        if (!isCooking)
        {
            isCooking = true;
            currentCookingTime = 0f;
        }

        currentCookingTime += Time.deltaTime;

        if (currentCookingTime >= cookingTime && currentCookingTime < burningTime)
        {
            // Pi�irme tamamland�
            pan.itemOnPan.GetComponent<Item>().SetCooked();
        }
        else if (currentCookingTime >= burningTime)
        {
            // Yanma durumu
            pan.itemOnPan.GetComponent<Item>().SetBurnt();
            isCooking = false;  // Pi�irme i�lemi durdurulur
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isOn);
            stream.SendNext(currentCookingTime);
        }
        else
        {
            isOn = (bool)stream.ReceiveNext();
            currentCookingTime = (float)stream.ReceiveNext();
            if (fireEffect != null) fireEffect.SetActive(isOn);  // Ate� efekti g�ncellenir
            if (stoveLight != null) stoveLight.enabled = isOn;  // I��k g�ncellenir
        }
    }
}
