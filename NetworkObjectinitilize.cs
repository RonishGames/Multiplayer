using UnityEngine;
using Unity.Netcode;
public class NetworkObjectinitilize : NetworkBehaviour
{
  //  public PrometeoCarController prometeoCarController;
    public Camera Playercamera;
    public GameObject PlayerCanvas;

    void Awake()
    {
        // prometeoCarController.enabled = false;
        Playercamera.enabled = false;
        PlayerCanvas.SetActive(false);
         this.transform.localPosition = new Vector3(1, 0, 0); //
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
             this.transform.localPosition = new Vector3(20, 1, -380); // Set initial position
            //  prometeoCarController.enabled = true;
            Playercamera.enabled = true;
            PlayerCanvas.SetActive(true);
            Debug.Log("PrometeoCarController enabled for owner");
           
        }
        else
        {

            this.transform.localPosition = new Vector3(-20, 1, -399); // Set initial position
        }
    }
}
