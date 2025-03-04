using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tray : MonoBehaviourPun
{
    public Transform[] itemSlots = new Transform[7]; // 7 slot
    private Stack<GameObject> items = new Stack<GameObject>(); // Ekleme sýrasýna göre takip için

    public void TryPickUpItem()
    {
        if (items.Count >= itemSlots.Length) return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 3f))
        {
            GameObject item = hit.collider.gameObject;
            if (!item.CompareTag("Pickable")) return;

            PhotonView itemView = item.GetComponent<PhotonView>();
            if (!itemView.IsMine) itemView.TransferOwnership(PhotonNetwork.LocalPlayer);

            int slotIndex = items.Count;
            photonView.RPC("RPC_AddItem", RpcTarget.AllBuffered, itemView.ViewID, slotIndex);
        }
    }

    [PunRPC]
    void RPC_AddItem(int itemViewID, int slotIndex)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);
        if (itemView == null) return;

        GameObject item = itemView.gameObject;
        item.transform.SetParent(itemSlots[slotIndex]);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        items.Push(item);
    }

    public void DropItem()
    {
        if (items.Count == 0) return;

        GameObject itemToDrop = items.Pop();
        photonView.RPC("RPC_DropItem", RpcTarget.AllBuffered, itemToDrop.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void RPC_DropItem(int itemViewID)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);
        if (itemView == null) return;

        GameObject item = itemView.gameObject;
        item.transform.SetParent(null);

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public bool IsEmpty()
    {
        return items.Count == 0;
    }
}
