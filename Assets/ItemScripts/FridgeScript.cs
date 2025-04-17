using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FridgeScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform fridgeDoor;
    public Transform fridgeShelf1;
    public Transform fridgeShelf2;
    public Light fridgeLight;
    private StorageSlot[] fridgeSlots;
    private bool isFridgeOpen = false;
    public float openCloseSpeed = 1f;
    public float interactionRange = 3f;

    private void Awake()
    {
        fridgeSlots = GetComponentsInChildren<StorageSlot>(true);

        // Baþlangýçta tüm slotlarý gizle
        foreach (var slot in fridgeSlots)
        {
            if (slot.slotVisual != null)
                slot.slotVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionRange))
        {
            if (hit.collider.CompareTag("Fridge"))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    photonView.RPC(isFridgeOpen ? "CloseFridge" : "OpenFridge", RpcTarget.AllBuffered);
                }
            }
            else if (hit.collider.CompareTag("FridgeSlot") && isFridgeOpen)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    var slot = hit.collider.GetComponent<StorageSlot>();
                    if (slot != null)
                    {
                        if (slot.IsOccupied)
                        {
                            // Eðer slot doluysa itemi çýkar
                            var item = slot.RetrieveItem();
                            if (item != null)
                            {
                                // Item'i oyuncuya verme mantýðý buraya
                                Debug.Log("Item retrieved: " + item.name);
                            }
                        }
                        else
                        {
                            // Slot boþsa item ekle
                            var item = GetItemInHand();
                            if (item != null && item.GetComponent<StorableItem>() != null)
                            {
                                photonView.RPC("PlaceItemInSlot", RpcTarget.AllBuffered,
                                    item.GetComponent<PhotonView>().ViewID,
                                    System.Array.IndexOf(fridgeSlots, slot));
                            }
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    void PlaceItemInSlot(int itemViewId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= fridgeSlots.Length) return;

        var item = PhotonView.Find(itemViewId)?.gameObject;
        if (item != null)
        {
            fridgeSlots[slotIndex].StoreItem(item);
        }
    }

    [PunRPC]
    void OpenFridge()
    {
        if (!isFridgeOpen)
        {
            isFridgeOpen = true;
            StopAllCoroutines();
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
        Vector3 targetDoorRotation = fridgeDoor.rotation.eulerAngles + new Vector3(0f, -90f, 0f);
        Vector3 targetShelfPosition1 = fridgeShelf1.position + Vector3.right * 0.5f;
        Vector3 targetShelfPosition2 = fridgeShelf2.position + Vector3.right * 0.5f;

        float journeyLength = Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f);
        float startTime = Time.time;

        fridgeLight.enabled = true;

        // Slot görsellerini aç (sadece boþ olanlar)
        foreach (var slot in fridgeSlots)
        {
            if (slot.slotVisual != null)
                slot.slotVisual.SetActive(!slot.IsOccupied);
        }

        while (Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * openCloseSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            fridgeDoor.rotation = Quaternion.Lerp(fridgeDoor.rotation, Quaternion.Euler(targetDoorRotation), fractionOfJourney);
            fridgeShelf1.position = Vector3.Lerp(fridgeShelf1.position, targetShelfPosition1, fractionOfJourney);
            fridgeShelf2.position = Vector3.Lerp(fridgeShelf2.position, targetShelfPosition2, fractionOfJourney);

            yield return null;
        }
    }

    IEnumerator CloseFridgeCoroutine()
    {
        Vector3 targetDoorRotation = fridgeDoor.rotation.eulerAngles - new Vector3(0f, -90f, 0f);
        Vector3 targetShelfPosition1 = fridgeShelf1.position - Vector3.right * 0.5f;
        Vector3 targetShelfPosition2 = fridgeShelf2.position - Vector3.right * 0.5f;

        float journeyLength = Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f);
        float startTime = Time.time;

        fridgeLight.enabled = false;

        // Tüm slot görsellerini kapat
        foreach (var slot in fridgeSlots)
        {
            if (slot.slotVisual != null)
                slot.slotVisual.SetActive(false);
        }

        while (Vector3.Distance(fridgeDoor.position, fridgeDoor.position + Vector3.right * 0.5f) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * openCloseSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            fridgeDoor.rotation = Quaternion.Lerp(fridgeDoor.rotation, Quaternion.Euler(targetDoorRotation), fractionOfJourney);
            fridgeShelf1.position = Vector3.Lerp(fridgeShelf1.position, targetShelfPosition1, fractionOfJourney);
            fridgeShelf2.position = Vector3.Lerp(fridgeShelf2.position, targetShelfPosition2, fractionOfJourney);

            yield return null;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isFridgeOpen);
        }
        else
        {
            isFridgeOpen = (bool)stream.ReceiveNext();
        }
    }

    private GameObject GetItemInHand()
    {
        // Burada oyuncunun elindeki itemi tespit etme mantýðýnýzý uygulayýn
        // Örnek: PlayerInventory.currentHeldItem gibi
        return null;
    }
}