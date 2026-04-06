using UnityEngine;

namespace GAME.FlowSys
{
    internal interface IMissionTransformReferenceRegistryPlaceholder
    {
        void RegisterReference(string id, MissionTransformReference reference);
        MissionTransformReference GetReference(string id);
        void Clear();
    }
}
