using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// Implémentation de la téléportation du joueur avec Vorta
    /// </summary>
    public class VortaPlayerTeleporter : MonoBehaviour, IMissionPlayerTeleporter
    {
        [Header("Player Rig")]
        [SerializeField] private Transform playerRig;
        
        public void Teleport(Vector3 position, Vector3 rotation)
        {
            if (playerRig == null)
            {
                Debug.LogError("[FlowSys] Cannot teleport: playerRig is null");
                return;
            }
            
            Debug.Log($"[FlowSys] Teleporting player to {position}, rotation {rotation}");
            
            playerRig.position = position;
            playerRig.rotation = Quaternion.Euler(rotation);
        }
        
        private void OnValidate()
        {
            if (playerRig == null)
            {
                GameObject rig = GameObject.Find("PlayerRig");
                if (rig == null)
                    rig = GameObject.Find("XR Origin");
                if (rig == null)
                    rig = GameObject.Find("XRRig");
                
                if (rig != null)
                    playerRig = rig.transform;
            }
        }
    }
}
