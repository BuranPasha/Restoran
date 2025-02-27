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

            PhotonView itemPhotonView = item.GetComponent<PhotonView>();
            if (itemPhotonView != null)
            {
                itemPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
        }
    }

    public GameObject TakeItem()
    {
        if (itemOnPan != null)
        {
            PhotonView itemPhotonView = itemOnPan.GetComponent<PhotonView>();
            if (itemPhotonView != null && itemPhotonView.IsMine)
            {
                itemPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                GameObject item = itemOnPan;
                itemOnPan = null;
                return item;
            }
        }
        return null;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(itemOnPan != null ? itemOnPan.transform.position : Vector3.zero);
            stream.SendNext(itemOnPan != null ? itemOnPan.transform.rotation : Quaternion.identity);
        }
        else
        {
            if (itemOnPan != null)
            {
                Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
                Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();
                itemOnPan.transform.position = receivedPosition;
                itemOnPan.transform.rotation = receivedRotation;
            }
        }
    }
}
