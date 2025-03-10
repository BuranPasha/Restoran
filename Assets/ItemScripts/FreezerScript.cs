using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FreezerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform freezerDoor; // Buzluðun kapak transformu
    public Light freezerLight; // Buzluðun ýþýðý
    public float openCloseSpeed = 1f; // Açýlma ve kapanma hýzý
    private bool isFreezerOpen = false; // Buzluðun açýk mý kapalý mý olduðu

    public float interactionRange = 3f; // Etkileþim mesafesi

    private void Update()
    {
        // Raycast ile bakýlan nesneyi tespit et
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionRange))
        {
            // Eðer bakýlan nesne buzdolabý ise
            if (hit.collider.CompareTag("Freezer"))
            {
                if (Input.GetKeyDown(KeyCode.E)) // E tuþuna basýldýðýnda
                {
                    if (isFreezerOpen)
                    {
                        photonView.RPC("CloseFreezer", RpcTarget.AllBuffered); // Tüm oyunculara buzluðu kapatma komutu gönderiyoruz
                    }
                    else
                    {
                        photonView.RPC("OpenFreezer", RpcTarget.AllBuffered); // Tüm oyunculara buzluðu açma komutu gönderiyoruz
                    }
                }
            }
        }
    }

    [PunRPC] // PhotonRPC komutlarý
    void OpenFreezer()
    {
        if (!isFreezerOpen)
        {
            isFreezerOpen = true;
            StopAllCoroutines(); // Eðer bir animasyon devam ediyorsa durduruyoruz
            StartCoroutine(OpenFreezerCoroutine());
        }
    }

    [PunRPC]
    void CloseFreezer()
    {
        if (isFreezerOpen)
        {
            isFreezerOpen = false;
            StopAllCoroutines();
            StartCoroutine(CloseFreezerCoroutine());
        }
    }

    IEnumerator OpenFreezerCoroutine()
    {
        // Kapak için hedef pozisyonu belirliyoruz (90 derece saða açýlacak)
        Vector3 targetDoorRotation = freezerDoor.rotation.eulerAngles + new Vector3(0f, -90f, 0f); // Kapak saða doðru açýlacak

        // Açýlma için süreyi hesaplýyoruz
        float journeyLength = Vector3.Distance(freezerDoor.position, freezerDoor.position + Vector3.right * 0.5f);
        float startTime = Time.time;

        while (Vector3.Distance(freezerDoor.position, freezerDoor.position + Vector3.right * 0.5f) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * openCloseSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            freezerDoor.rotation = Quaternion.Lerp(freezerDoor.rotation, Quaternion.Euler(targetDoorRotation), fractionOfJourney);

            yield return null;
        }

        freezerLight.enabled = true; // Iþýðý açýyoruz
    }

    IEnumerator CloseFreezerCoroutine()
    {
        // Kapak için hedef pozisyonu belirliyoruz (geri 0 derece)
        Vector3 targetDoorRotation = freezerDoor.rotation.eulerAngles - new Vector3(0f, -90f, 0f); // Kapak geri kapanacak

        // Kapanma için süreyi hesaplýyoruz
        float journeyLength = Vector3.Distance(freezerDoor.position, freezerDoor.position + Vector3.right * 0.5f);
        float startTime = Time.time;

        while (Vector3.Distance(freezerDoor.position, freezerDoor.position + Vector3.right * 0.5f) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * openCloseSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            freezerDoor.rotation = Quaternion.Lerp(freezerDoor.rotation, Quaternion.Euler(targetDoorRotation), fractionOfJourney);

            yield return null;
        }

        freezerLight.enabled = false; // Iþýðý kapatýyoruz
    }

    // Photon'dan gelen veriyi senkronize ediyoruz
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Veri yazma iþlemi
        {
            stream.SendNext(isFreezerOpen); // Buzluðun açýk/kapalý durumu
        }
        else // Veri okuma iþlemi
        {
            isFreezerOpen = (bool)stream.ReceiveNext();
        }
    }
}
