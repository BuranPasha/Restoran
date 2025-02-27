using Photon.Pun;
using UnityEngine;

public class Pan : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject itemOnPan;
    public Stove stove;

    public void PlaceItem(GameObject item)
    {
        if (itemOnPan == null && item != null)
        {
            itemOnPan = item;
            item.transform.position = transform.position;
            item.SetActive(true);

            PhotonView itemPV = item.GetComponent<PhotonView>();
            if (itemPV != null)
            {
                itemPV.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            // Et pan �zerine konuldu�unda, peki e�er et �i� ise pi�irmeye ba�las�n.
            Item itemComponent = item.GetComponent<Item>();
            if (itemComponent != null)
            {
                itemComponent.ResumeCooking();
            }

            photonView.RPC("RPC_SetItemOnPan", RpcTarget.AllBuffered, item.GetComponent<PhotonView>().ViewID);
        }
    }

    public GameObject TakeItem()
    {
        if (itemOnPan != null)
        {
            PhotonView itemPV = itemOnPan.GetComponent<PhotonView>();
            if (itemPV != null && itemPV.IsMine)
            {
                GameObject item = itemOnPan;
                // Pan�daki item referans�n� a� �zerinde temizle.
                photonView.RPC("RPC_ClearItemOnPan", RpcTarget.AllBuffered);

                // Et pan�dan al�nd���nda pi�irme duraklas�n.
                Item itemComponent = item.GetComponent<Item>();
                if (itemComponent != null)
                {
                    itemComponent.PauseCooking();
                }

                // Ocak timer��n� resetleyelim ki et pi�irme s�reci durdu.
                if (stove != null)
                {
                    stove.ResetCooking();
                }

                return item;
            }
        }
        return null;
    }

    [PunRPC]
    void RPC_ClearItemOnPan()
    {
        itemOnPan = null;
    }

    [PunRPC]
    void RPC_SetItemOnPan(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            itemOnPan = pv.gameObject;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool hasItem = (itemOnPan != null);
            stream.SendNext(hasItem);
            if (hasItem)
            {
                stream.SendNext(itemOnPan.GetComponent<PhotonView>().ViewID);
            }
        }
        else
        {
            bool hasItem = (bool)stream.ReceiveNext();
            if (hasItem)
            {
                int viewID = (int)stream.ReceiveNext();
                PhotonView pv = PhotonView.Find(viewID);
                if (pv != null)
                {
                    itemOnPan = pv.gameObject;
                }
            }
            else
            {
                itemOnPan = null;
            }
        }
    }
}
