using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class StackableItem : MonoBehaviourPun, IPunObservable
{
    [Header("Stack Settings")]
    public string itemType = "Default";
    public Vector3 holdRotation = new Vector3(90, 0, 0);
    [HideInInspector] public bool isStacked = false;
    [HideInInspector] public Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isStacked);
            stream.SendNext(originalScale);
        }
        else
        {
            isStacked = (bool)stream.ReceiveNext();
            originalScale = (Vector3)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void SetStacked(bool stacked)
    {
        isStacked = stacked;

        if (stacked)
        {
            // Stacklendiðinde görünürlüðü garanti et
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.SetParent(null);
            transform.localScale = originalScale;
        }
    }
}