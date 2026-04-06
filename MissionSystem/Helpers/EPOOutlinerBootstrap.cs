using EPOOutline;
using UnityEngine;

namespace GAME.MissionSystem
{
    [DisallowMultipleComponent]
    public class EPOOutlinerBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Camera cam in cameras)
            {
                if (cam.GetComponent<Outliner>() == null)
                    cam.gameObject.AddComponent<Outliner>();
            }
        }
    }
}
