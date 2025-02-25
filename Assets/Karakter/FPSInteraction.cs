using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;  // Etkile�im mesafesi
    public Transform holdPosition;  // Oyuncunun el pozisyonu
    private GameObject heldObject = null;  // �u anda tutulan nesne
    private Vector3 originalScale;  // Itemin orijinal scale'ini saklamak i�in bir de�i�ken
    private bool isItemHeld = false;  // Item tutma durumu (local oyuncu i�in)
    private Rigidbody heldRigidbody;  // Tutulan nesnenin Rigidbody bile�eni
    private PhotonView heldObjectPhotonView;  // Nesnenin PhotonView bile�eni

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
        {
            if (heldObject == null)
            {
                TryPickUp();
            }
            else
            {
                DropObject();
            }
        }
    }

    void TryPickUp()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Pickable"))
            {
                GameObject objectToPickUp = hit.collider.gameObject;
                PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();

                // E�er nesne ba�ka bir oyuncu taraf�ndan al�nd�ysa i�lem yapma
                if (objectPhotonView.Owner != null && !objectPhotonView.IsMine)
                {
                    return; // Ba�ka biri tutuyorsa itemi alamay�z.
                }

                // Sahipli�i al ve itemi tut
                objectPhotonView.RequestOwnership();
                heldObject = objectToPickUp;
                heldObjectPhotonView = objectPhotonView;
                heldRigidbody = heldObject.GetComponent<Rigidbody>();
                originalScale = heldObject.transform.localScale;

                PickUpObject(heldObject);
            }
        }
    }

    void PickUpObject(GameObject objectToPickUp)
    {
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        objectToPickUp.transform.SetParent(holdPosition);
        objectToPickUp.transform.localPosition = Vector3.zero;
        objectToPickUp.transform.localRotation = Quaternion.Euler(0, 90, 0);
        objectToPickUp.transform.localScale = originalScale;

        isItemHeld = true;
    }

    void DropObject()
    {
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
            heldRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        heldObject.transform.SetParent(null);
        heldObject = null;
        isItemHeld = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isItemHeld);
            stream.SendNext(heldObject != null ? heldObject.transform.position : Vector3.zero);
            stream.SendNext(heldObject != null ? heldObject.transform.rotation : Quaternion.identity);
        }
        else
        {
            isItemHeld = (bool)stream.ReceiveNext();
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
