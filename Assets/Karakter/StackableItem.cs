using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class StackableItem : MonoBehaviourPun, IPunObservable
{
    [Header("Stack Settings")]
    public string itemType = "Default";
    public Vector3 holdRotation = new Vector3(90, 0, 0);
    [HideInInspector] public bool isStacked = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isStacked);
        }
        else
        {
            isStacked = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void SetStacked(bool stacked)
    {
        isStacked = stacked;

        if (!stacked && transform.parent != null)
        {
            transform.SetParent(null);
        }
    }
}