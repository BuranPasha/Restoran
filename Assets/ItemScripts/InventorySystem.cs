using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class InventorySystem : MonoBehaviourPunCallbacks, IPunObservable
{
    public int inventorySize = 5; // Maksimum 5 slot
    private List<GameObject> inventory = new List<GameObject>();

    // Photon'a item eklemek
    public bool AddItem(GameObject item)
    {
        if (inventory.Count < inventorySize)
        {
            inventory.Add(item);
            item.SetActive(false); // Eþyayý sahneden kaldýr
            photonView.RPC("RPC_AddItem", RpcTarget.AllBuffered, item.GetComponent<PhotonView>().ViewID);
            return true;
        }
        else
        {
            Debug.Log("Envanter dolu!");
            return false;
        }
    }

    // Photon'a item kaldýrmak
    public void RemoveItem(int slotIndex, Transform dropPosition)
    {
        if (slotIndex >= 0 && slotIndex < inventory.Count)
        {
            GameObject item = inventory[slotIndex];
            inventory.RemoveAt(slotIndex);
            item.SetActive(true);
            item.transform.position = dropPosition.position;
            photonView.RPC("RPC_RemoveItem", RpcTarget.AllBuffered, item.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            Debug.Log("Geçersiz slot numarasý!");
        }
    }

    // Photon'a item silme iþlemi
    [PunRPC]
    void RPC_AddItem(int viewID)
    {
        PhotonView itemView = PhotonView.Find(viewID);
        if (itemView != null)
        {
            GameObject item = itemView.gameObject;
            inventory.Add(item);
            item.SetActive(false);  // Envantere eklendiði için sahneden gizlenir
        }
    }

    // Photon'a item kaldýrma iþlemi
    [PunRPC]
    void RPC_RemoveItem(int viewID)
    {
        PhotonView itemView = PhotonView.Find(viewID);
        if (itemView != null)
        {
            GameObject item = itemView.gameObject;
            item.SetActive(true);
            inventory.Remove(item);
        }
    }

    // Photon'dan gelen veriyi senkronize ediyoruz
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Envanter verisi gönderilecek
            stream.SendNext(inventory.Count);
            foreach (var item in inventory)
            {
                stream.SendNext(item.GetComponent<PhotonView>().ViewID);
            }
        }
        else
        {
            // Envanter verisi alýnacak
            int itemCount = (int)stream.ReceiveNext();
            inventory.Clear();

            for (int i = 0; i < itemCount; i++)
            {
                int viewID = (int)stream.ReceiveNext();
                PhotonView itemView = PhotonView.Find(viewID);
                if (itemView != null)
                {
                    inventory.Add(itemView.gameObject);
                }
            }
        }
    }
}
