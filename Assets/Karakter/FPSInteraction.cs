using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;
    public Transform holdPosition;
    public string leftPositionTag = "LeftPosition"; // Býrakma noktasýnýn etiketi

    private GameObject heldObject = null;
    private Rigidbody heldRigidbody;
    private PhotonView heldObjectPhotonView;

    private GameObject[] inventory = new GameObject[5]; // 5 slotlu envanter

    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.E)) // E tuþu ile nesne al
            {
                if (heldObject == null)
                {
                    TryPickUp();
                }
            }
            else if (Input.GetKeyDown(KeyCode.G)) // G tuþu ile nesne býrak
            {
                if (heldObject != null)
                {
                    TryDropOrPlace();
                }
            }

            // Envantere ekleme (R tuþu ile elindekini envantere koy)
            if (Input.GetKeyDown(KeyCode.R) && heldObject != null)
            {
                StoreItemInInventory();
            }

            // Envanterden nesne alma (1-5 tuþlarý)
            for (int i = 0; i < inventory.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    RetrieveItemFromInventory(i);
                }
            }
        }
    }

    void TryPickUp()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject objectToPickUp = hit.collider.gameObject;

            if (!objectToPickUp.CompareTag("Pickable"))
            {
                Debug.Log("Bu nesne alýnamaz: " + objectToPickUp.name);
                return;
            }

            PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();

            if (objectPhotonView == null)
            {
                Debug.Log("Nesne PhotonView bileþenine sahip deðil: " + objectToPickUp.name);
                return;
            }

            if (!objectPhotonView.IsMine)
            {
                objectPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, objectPhotonView.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    void TryDropOrPlace()
    {
        GameObject leftPosition = GameObject.FindGameObjectWithTag(leftPositionTag);

        if (leftPosition != null && Vector3.Distance(transform.position, leftPosition.transform.position) <= interactionDistance)
        {
            photonView.RPC("RPC_PlaceOnLeftPosition", RpcTarget.AllBuffered, heldObjectPhotonView.ViewID, leftPosition.transform.position);
        }
        else
        {
            DropObject();
        }
    }

    void StoreItemInInventory()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null) // Boþ slot bulursa
            {
                inventory[i] = heldObject;
                heldObject.SetActive(false); // Envantere alýnan objeyi gizle
                heldObject = null;
                heldObjectPhotonView = null;
                heldRigidbody = null;
                Debug.Log("Nesne envantere eklendi: Slot " + (i + 1));
                return;
            }
        }
        Debug.Log("Envanter dolu!");
    }

    void RetrieveItemFromInventory(int slotIndex)
    {
        if (inventory[slotIndex] != null && heldObject == null) // Slot doluysa ve elimizde nesne yoksa
        {
            heldObject = inventory[slotIndex];
            inventory[slotIndex] = null;
            heldObject.SetActive(true); // Objeyi tekrar görünür yap
            photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, heldObject.GetComponent<PhotonView>().ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
            Debug.Log("Nesne eline alýndý: Slot " + (slotIndex + 1));
        }
    }

    [PunRPC]
    void RPC_PickUpObject(int objectViewID, int ownerActorNumber)
    {
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null)
        {
            Debug.LogError("PhotonView bulunamadý: " + objectViewID);
            return;
        }

        GameObject objectToPickUp = foundView.gameObject;
        PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();

        if (!objectPhotonView.IsMine)
        {
            objectPhotonView.TransferOwnership(ownerActorNumber);
        }

        heldObject = objectToPickUp;
        heldObjectPhotonView = heldObject.GetComponent<PhotonView>();
        heldRigidbody = heldObject.GetComponent<Rigidbody>();

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.freezeRotation = true;
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
    }

    void DropObject()
    {
        if (heldObject == null) return;

        if (heldObjectPhotonView != null && heldObjectPhotonView.IsMine)
        {
            heldObjectPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
        }

        photonView.RPC("RPC_DropObject", RpcTarget.AllBuffered, heldObjectPhotonView.ViewID);
    }

    [PunRPC]
    void RPC_DropObject(int objectViewID)
    {
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null)
        {
            Debug.LogError("PhotonView bulunamadý: " + objectViewID);
            return;
        }

        GameObject droppedObject = foundView.gameObject;
        Rigidbody droppedRigidbody = droppedObject.GetComponent<Rigidbody>();

        if (droppedRigidbody != null)
        {
            droppedRigidbody.isKinematic = false;
            droppedRigidbody.useGravity = true;
        }

        droppedObject.transform.SetParent(null);
        droppedObject.transform.rotation = Quaternion.identity;

        heldObject = null;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(heldObject != null ? heldObject.transform.position : Vector3.zero);
            stream.SendNext(heldObject != null ? heldObject.transform.rotation : Quaternion.identity);
        }
        else
        {
            Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();

            if (heldObject != null)
            {
                heldObject.transform.position = receivedPosition;
                heldObject.transform.rotation = receivedRotation;
            }
        }
    }
}
