using System.Collections.Generic;
using UnityEngine;

namespace GAME.MissionSystem
{
    internal class MissionTransformReferenceRegistryStandalonePlaceholder : MonoBehaviour
    {
        private readonly Dictionary<string, MissionTransformReference> references = new Dictionary<string, MissionTransformReference>();
    }
}
