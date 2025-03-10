using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FridgeScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform fridgeDoor; // Buzdolabýnýn kapak transformu
    public Transform fridgeShelf1; // Raf 1
    public Transform fridgeShelf2; // Raf 2
    public Light fridgeLight; // Buzdolabýnýn ýþýðý

    private bool isFridgeOpen = false; // Buzdolabýnýn açýk mý kapalý mý olduðu
    public float openCloseSpeed = 1f; // Açýlma ve kapanma hýzý

    public float interactionRange = 3f; // Etkileþim mesafesi

    private void Update()
    {
        // Raycast ile bakýlan nesneyi tespit et
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionRange))
        {
            // Eðer bakýlan nesne buzdolabý ise
            if (hit.collider.CompareTag("Fridge"))
            {
                if (Input.GetKeyDown(KeyCode.E)) // E tuþuna basýldýðýnda
                {
                    if (isFridgeOpen)
                    {
                        photonView.RPC("CloseFridge", RpcTarget.AllBuffered); // Tüm oyunculara buzdolabýný kapatma komutu gönderiyoruz
                    }
                    else
                    {
                        photonView.RPC("OpenFridge", RpcTarget.AllBuffered); // Tüm oyunculara buzdolabýný açma komutu gönderiyoruz
                    }
                }
            }
        }
    }

    [PunRPC] // PhotonRPC komutlarý
    void OpenFridge()
    {
        if (!isFridgeOpen)
        {
            isFridgeOpen = true;
            StopAllCoroutines(); // Eðer bir animasyon devam ediyorsa durduruyoruz
            StartCoroutine(OpenFridgeCoroutine());
        }
    }

    [PunRPC]
    void CloseFridge()
    {
        if (isFridgeOpen)
        {
            isFridgeOpen = false;
            StopAllCoroutines();
            StartCoroutine(CloseFridgeCoroutine());
        }
    }

    IEnumerator OpenFridgeCoroutine()
    {
        // Kapak için hedef pozisyonu belirliyoruz (90 derece saða açýlacak)
        Vector3 targetDoorRotation = fridgeDoor.rotation.eulerAngles + new Vector3(0f, -90f, 0f); // Kapak saða doðru açýlacak

        // Raflar için hareket pozisyonlarý
        Vector3 targetShelfPosition1 = fridgeShelf1.position + Vector3.right * 0.5f;
        Vector3 targetShelfPosition2 = fridgeShelf2.position + Vector3.right * 0.5f;

        // Açýlma için süreyi hesaplýyoruz
        float journeyLength = Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f);
        float startTime = Time.time;

        while (Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * openCloseSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            fridgeDoor.rotation = Quaternion.Lerp(fridgeDoor.rotation, Quaternion.Euler(targetDoorRotation), fractionOfJourney);
            fridgeShelf1.position = Vector3.Lerp(fridgeShelf1.position, targetShelfPosition1, fractionOfJourney);
            fridgeShelf2.position = Vector3.Lerp(fridgeShelf2.position, targetShelfPosition2, fractionOfJourney);

            yield return null;
        }

        fridgeLight.enabled = true; // Iþýðý açýyoruz
    }

    IEnumerator CloseFridgeCoroutine()
    {
        // Kapak için hedef pozisyonu belirliyoruz (geri 0 derece)
        Vector3 targetDoorRotation = fridgeDoor.rotation.eulerAngles - new Vector3(0f, -90f, 0f); // Kapak geri kapanacak

        // Raflar için hareket pozisyonlarý
        Vector3 targetShelfPosition1 = fridgeShelf1.position - Vector3.right * 0.5f;
        Vector3 targetShelfPosition2 = fridgeShelf2.position - Vector3.right * 0.5f;

        // Kapanma için süreyi hesaplýyoruz
        float journeyLength = Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f);
        float startTime = Time.time;

        while (Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * openCloseSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            fridgeDoor.rotation = Quaternion.Lerp(fridgeDoor.rotation, Quaternion.Euler(targetDoorRotation), fractionOfJourney);
            fridgeShelf1.position = Vector3.Lerp(fridgeShelf1.position, targetShelfPosition1, fractionOfJourney);
            fridgeShelf2.position = Vector3.Lerp(fridgeShelf2.position, targetShelfPosition2, fractionOfJourney);

            yield return null;
        }

        fridgeLight.enabled = false; // Iþýðý kapatýyoruz
    }

    // Photon'dan gelen veriyi senkronize ediyoruz
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Veri yazma iþlemi
        {
            stream.SendNext(isFridgeOpen); // Buzdolabýnýn açýk/kapalý durumu
        }
        else // Veri okuma iþlemi
        {
            isFridgeOpen = (bool)stream.ReceiveNext();
        }
    }
}
