using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class TableScript : MonoBehaviourPun
{
    // Masa �zerindeki slotlar
    public Transform yemekSlot;
    public Transform tatliSlot;
    public Transform salataSlot;
    public Transform icecekSlot;
    public Transform catalSlot;
    public Transform kasikSlot;
    public Transform b��akSlot;

    private Dictionary<string, Transform> slotDictionary;

    // Ba�lang��ta her slotu dictionary'ye ekleyelim
    void Start()
    {
        slotDictionary = new Dictionary<string, Transform>
        {
            { "Yemek", yemekSlot },
            { "Tatl�", tatliSlot },
            { "Salata", salataSlot },
            { "��ecek", icecekSlot },
            { "�atal", catalSlot },
            { "Ka��k", kasikSlot },
            { "B��ak", b��akSlot }
        };
    }

    [PunRPC]
    public void RPC_PlaceItemsFromTray(int[] itemViewIDs)
    {
        // Tepsiden item'lar� al�p, masa �zerindeki do�ru slot'a yerle�tiriyoruz
        for (int i = 0; i < itemViewIDs.Length; i++)
        {
            if (itemViewIDs[i] == -1) continue; // Bo� slot

            PhotonView itemView = PhotonView.Find(itemViewIDs[i]);
            if (itemView == null) continue;

            GameObject item = itemView.gameObject;
            ItemScript itemScript = item.GetComponent<ItemScript>();

            if (itemScript != null)
            {
                // ItemType'a g�re do�ru slot'u bulup item'� oraya yerle�tiriyoruz
                string itemType = itemScript.itemType.ToString();
                if (slotDictionary.ContainsKey(itemType))
                {
                    Transform targetSlot = slotDictionary[itemType];
                    item.transform.SetParent(targetSlot);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;

                    Rigidbody rb = item.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;
                }
            }
        }
    }

    public bool HasItemsOnTable()
    {
        // Masa �zerinde herhangi bir item olup olmad���n� kontrol ediyoruz
        foreach (Transform slot in slotDictionary.Values)
        {
            if (slot.childCount > 0) return true; // E�er herhangi bir slotta item varsa true d�ner
        }
        return false;
    }
}
