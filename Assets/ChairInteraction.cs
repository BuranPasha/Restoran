using UnityEngine;
using Photon.Pun;

public class ChairInteraction : MonoBehaviourPunCallbacks
{
    private bool isSitting = false;
    private Transform player;
    private Transform chairPosition;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private CharacterController playerController;
    private Rigidbody playerRigidbody;
    private PhotonView photonView;

    void Start()
    {
        chairPosition = transform.Find("SitPoint");
        if (chairPosition == null)
        {
            Debug.LogError("SitPoint nesnesi bulunamadý! Sandalyenin içine boþ bir GameObject ekleyip 'SitPoint' adýný ver.");
        }

        photonView = GetComponent<PhotonView>();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E tuþuna basýldý! isSitting = " + isSitting);

                if (!isSitting)
                {
                    photonView.RPC("Sit", RpcTarget.AllBuffered, other.GetComponent<PhotonView>().ViewID);
                }
                else
                {
                    photonView.RPC("StandUp", RpcTarget.AllBuffered);
                }
            }
        }
    }

    [PunRPC]
    void Sit(int playerID)
    {
        player = PhotonView.Find(playerID).transform;
        isSitting = true;

        Debug.Log("Sit() çaðrýldý! Oyuncu oturdu.");

        originalPosition = player.position;
        originalRotation = player.rotation;

        playerController = player.GetComponent<CharacterController>();
        playerRigidbody = player.GetComponent<Rigidbody>();

        // Oturma iþlemi: CharacterController'ý devre dýþý býrak ve Rigidbody'yi kinematik yap
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }

        player.position = chairPosition.position;
        player.rotation = chairPosition.rotation;
        player.SetParent(chairPosition);
    }

    [PunRPC]
    void StandUp()
    {
        Debug.Log("StandUp() çaðrýldý! Oyuncu kalkýyor...");

        if (player != null)
        {
            isSitting = false;
            player.SetParent(null);

            Vector3 newPosition = originalPosition + new Vector3(0, 0, 0.5f);
            player.position = newPosition;
            player.rotation = originalRotation;

            // Kalkma iþlemi: CharacterController'ý tekrar aktif et ve Rigidbody'yi normal hale getir
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;
            }

            player = null;
        }
    }
}
