using Photon.Pun;
using UnityEngine;

public class Chair : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform sitPoint; // Sandalyede oturulacak nokta

    private PlayerMovement sittingPlayer = null; // Sandalyede oturan oyuncu
    private bool isOccupied = false; // Sandalye dolu mu?

    public void SitOrStand(PlayerMovement player)
    {
        if (!gameObject.CompareTag("Chair"))
        {
            return;
        }

        if (photonView.IsMine)
        {
            if (!isOccupied)
            {
                Sit(player);
            }
            else if (sittingPlayer == player)
            {
                StandUp();
            }
        }
    }

    void Sit(PlayerMovement player)
    {
        sittingPlayer = player;
        isOccupied = true;

        player.characterController.enabled = false; // Hareketi kapat

        if (sitPoint != null)
        {
            // Karakterin pozisyonunu SitPosition noktasýna taþý
            player.transform.position = sitPoint.position;
            player.transform.rotation = sitPoint.rotation;
        }

        player.isSitting = true;

        // Durumu tüm oyunculara senkronize et
        photonView.RPC("RPC_Sit", RpcTarget.All, player.photonView.ViewID);
    }

    void StandUp()
    {
        if (sittingPlayer != null)
        {
            sittingPlayer.characterController.enabled = true; // Hareketi aç
            sittingPlayer.isSitting = false;
            sittingPlayer = null;
            isOccupied = false;

            // Durumu tüm oyunculara senkronize et
            photonView.RPC("RPC_StandUp", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_Sit(int playerViewID)
    {
        PlayerMovement player = PhotonView.Find(playerViewID).GetComponent<PlayerMovement>();
        sittingPlayer = player;
        isOccupied = true;

        player.characterController.enabled = false;

        if (sitPoint != null)
        {
            player.transform.position = sitPoint.position;
            player.transform.rotation = sitPoint.rotation;
        }

        player.isSitting = true;
    }

    [PunRPC]
    void RPC_StandUp()
    {
        if (sittingPlayer != null)
        {
            sittingPlayer.characterController.enabled = true;
            sittingPlayer.isSitting = false;
            sittingPlayer = null;
            isOccupied = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isOccupied);
            stream.SendNext(sittingPlayer != null ? sittingPlayer.photonView.ViewID : -1);
        }
        else
        {
            isOccupied = (bool)stream.ReceiveNext();
            int playerViewID = (int)stream.ReceiveNext();

            if (playerViewID != -1)
            {
                sittingPlayer = PhotonView.Find(playerViewID).GetComponent<PlayerMovement>();
            }
            else
            {
                sittingPlayer = null;
            }
        }
    }
}