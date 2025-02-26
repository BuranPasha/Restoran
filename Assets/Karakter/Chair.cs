using UnityEngine;

public class Chair : MonoBehaviour
{
    public Transform sitPoint; // Sandalyede oturulacak nokta

    private PlayerMovement sittingPlayer = null;

    public void SitOrStand(PlayerMovement player)
    {
        if (!gameObject.CompareTag("Chair"))
        {
            
            return;
        }

        if (sittingPlayer == null)
        {
            Sit(player);
        }
        else if (sittingPlayer == player)
        {
            StandUp();
        }
    }

    void Sit(PlayerMovement player)
    {
        
        sittingPlayer = player;

        player.characterController.enabled = false; // Hareketi kapat

        if (sitPoint != null)
        {
            // Karakterin pozisyonunu SitPosition noktasýna taþý
            player.transform.position = sitPoint.position;
            player.transform.rotation = sitPoint.rotation;
        }
        else
        {
            
        }

        player.isSitting = true;
    }

    void StandUp()
    {
        if (sittingPlayer != null)
        {
            
            sittingPlayer.characterController.enabled = true; // Hareketi aç

            sittingPlayer.isSitting = false;
            sittingPlayer = null;
        }
    }
}