using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class WaiterSystem : MonoBehaviourPunCallbacks
{
    public Transform holdPosition; // Tepsiyi tutma noktas�
    public string trayTag = "Tray"; // Tepsinin etiketi
    public float interactionDistance = 3f; // Etkile�im mesafesi

    private GameObject heldTray = null; // Eldeki tepsi
    private Dictionary<string, GameObject> traySlots = new Dictionary<string, GameObject>(); // Tepsideki ��eler

    private readonly string[] slotNames = { "Yemek", "Tatl�", "Salata", "��ecek", "�atal", "Ka��k", "B��ak" };

    void Start()
    {
        foreach (string slot in slotNames)
        {
            traySlots[slot] = null; // Ba�lang��ta t�m slotlar bo�
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldTray == null)
                TryPickUpTray(); // Tepsiyi al
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TryPlaceItemOnTray(); // ��eyi tepsiye koy
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            TryDropItemFromTray(); // Tepsiden s�rayla b�rak
        }
    }

    void TryPickUpTray()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, interactionDistance))
        {
            GameObject tray = hit.collider.gameObject;
            if (!tray.CompareTag(trayTag)) return;

            PhotonView trayPhotonView = tray.GetComponent<PhotonView>();
            if (trayPhotonView == null) return;

            if (!trayPhotonView.IsMine)
                trayPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);

            photonView.RPC("RPC_PickUpTray", RpcTarget.AllBuffered, trayPhotonView.ViewID);
        }
    }

    [PunRPC]
    void RPC_PickUpTray(int trayViewID)
    {
        PhotonView trayView = PhotonView.Find(trayViewID);
        if (trayView == null) return;

        heldTray = trayView.gameObject;
        heldTray.transform.SetParent(holdPosition);
        heldTray.transform.localPosition = Vector3.zero;
        heldTray.transform.localRotation = Quaternion.identity;

        Rigidbody rb = heldTray.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void DropTray()
    {
        if (heldTray == null) return;

        photonView.RPC("RPC_DropTray", RpcTarget.AllBuffered, heldTray.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void RPC_DropTray(int trayViewID)
    {
        PhotonView trayView = PhotonView.Find(trayViewID);
        if (trayView == null) return;

        GameObject tray = trayView.gameObject;
        tray.transform.SetParent(null);

        Rigidbody rb = tray.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        heldTray = null;
    }

    void TryPlaceItemOnTray()
    {
        if (heldTray == null) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, interactionDistance))
        {
            GameObject item = hit.collider.gameObject;

            ItemScript itemScript = item.GetComponent<ItemScript>();
            if (itemScript == null) return;

            string itemName = itemScript.GetItemSlotName();
            if (itemName == null || traySlots[itemName] != null) return; // Bo� bir slot yoksa ekleme

            PhotonView itemView = item.GetComponent<PhotonView>();
            if (itemView == null) return;

            if (!itemView.IsMine)
                itemView.TransferOwnership(PhotonNetwork.LocalPlayer);

            photonView.RPC("RPC_PlaceItemOnTray", RpcTarget.AllBuffered, itemView.ViewID, itemName);
        }
    }

    [PunRPC]
    void RPC_PlaceItemOnTray(int itemViewID, string slotName)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);
        if (itemView == null) return;

        GameObject item = itemView.gameObject;
        traySlots[slotName] = item;

        // ��eyi do�ru slot'a yerle�tirmek i�in, ��e ismine g�re do�ru pozisyona ta��yoruz
        Transform targetSlot = GetSlotTransform(slotName);
        if (targetSlot != null)
        {
            item.transform.SetParent(targetSlot);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void TryDropItemFromTray()
    {
        if (heldTray == null) return;

        foreach (string slot in slotNames)
        {
            if (traySlots[slot] != null)
            {
                photonView.RPC("RPC_DropItemFromTray", RpcTarget.AllBuffered, traySlots[slot].GetComponent<PhotonView>().ViewID, slot);
                return;
            }
        }
        DropTray(); // T�m itemler b�rak�ld�ysa tepsiyi b�rak
    }

    [PunRPC]
    void RPC_DropItemFromTray(int itemViewID, string slotName)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);
        if (itemView == null) return;

        GameObject item = itemView.gameObject;
        traySlots[slotName] = null;

        item.transform.SetParent(null);
        item.transform.position = heldTray.transform.position + Vector3.forward * 0.5f;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    // Slot ad�yla uyumlu olan do�ru transform'u d�nd�ren metot
    Transform GetSlotTransform(string slotName)
    {
        // Burada her bir slot'un transform referans�n� d�nd�rmelisiniz.
        // �rne�in, "Salata" slotu i�in 'SalataSlot' ad�nda bir transform varsa:
        Transform slotTransform = GameObject.Find(slotName + "Slot")?.transform;

        return slotTransform;
    }
}
