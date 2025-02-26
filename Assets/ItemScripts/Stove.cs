using Photon.Pun;
using UnityEngine;
using static UnityEditor.Progress;

public class Stove : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isOn = false;  // Ocak a��k m� kapal� m�?
    public GameObject fireEffect;  // Ocak a��kken g�sterilecek ate� efekti
    public Pan pan;  // Tava referans� (tavay� oca�a yerle�tirmek i�in)
    public float cookingTime = 10f;  // Pi�irme s�resi
    private float currentCookingTime = 0f;  // Ge�en pi�irme s�resi
    private bool isCooking = false;  // Pi�irme i�lemi devam ediyor mu?

    void Update()
    {
        // Sadece oca��n sahibi oca�� a��p kapatabilir
        if (Input.GetKeyDown(KeyCode.F) && photonView.IsMine)
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
        fireEffect.SetActive(isOn);  // Ate� efekti a��k/kapal� durumuna g�re g�ncellenir

        if (!isOn)
        {
            // Ocak kapat�ld���nda pi�irme s�resini s�f�rla
            currentCookingTime = 0f;
            isCooking = false;
        }
    }

    void CookItem()
    {
        if (!isCooking)
        {
            isCooking = true;
            currentCookingTime = 0f;
        }

        currentCookingTime += Time.deltaTime;

        if (currentCookingTime >= cookingTime)
        {
            // Pi�irme tamamland�
            pan.itemOnPan.GetComponent<Item>().SetCooked();
            isCooking = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Ocak durumunu senkronize et
            stream.SendNext(isOn);
            stream.SendNext(currentCookingTime);
        }
        else
        {
            // Ocak durumunu g�ncelle
            isOn = (bool)stream.ReceiveNext();
            currentCookingTime = (float)stream.ReceiveNext();
            fireEffect.SetActive(isOn);  // Ate� efekti g�ncellenir
        }
    }
}