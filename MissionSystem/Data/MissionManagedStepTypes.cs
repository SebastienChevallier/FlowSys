using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MissionActionCategoryAttribute : Attribute
    {
        public string Category { get; }

        public MissionActionCategoryAttribute(string category)
        {
            Category = category;
        }
    }

    [Serializable]
    public abstract class MissionActionData
    {
        public abstract string GetDisplayName();
        public abstract string GetTypeName();
        public abstract bool IsAsync { get; }
        public abstract void Execute(IMissionContext context, Action onComplete);
        public virtual void HandleStepExit(IMissionContext context)
        {
        }
    }

    [Serializable]
    public abstract class MissionConditionData
    {
        public abstract string GetDisplayName();
        public abstract string GetTypeName();
        public abstract bool Evaluate(IMissionContext context);
    }

    public static class MissionRuntimeResolver
    {
        public static MissionTransformReference FindTransformReference(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId))
                return null;

            MissionTransformReferenceRegistry registry = MissionTransformReferenceRegistry.Instance;
            if (registry != null)
            {
                MissionTransformReference registeredReference = registry.GetReference(referenceId);
                if (registeredReference != null)
                    return registeredReference;
            }

            MissionTransformReference[] allReferences = UnityEngine.Object.FindObjectsOfType<MissionTransformReference>();
            for (int i = 0; i < allReferences.Length; i++)
            {
                MissionTransformReference reference = allReferences[i];
                if (reference != null && reference.referenceId == referenceId)
                    return reference;
            }

            MissionTransformReference[] loadedReferences = Resources.FindObjectsOfTypeAll<MissionTransformReference>();
            for (int i = 0; i < loadedReferences.Length; i++)
            {
                MissionTransformReference reference = loadedReferences[i];
                if (reference == null)
                    continue;

                if (!reference.gameObject.scene.IsValid() || !reference.gameObject.scene.isLoaded)
                    continue;

                if (reference.hideFlags != HideFlags.None)
                    continue;

                if (reference.referenceId == referenceId)
                    return reference;
            }

            return null;
        }

        public static MissionSceneLoader FindSceneLoader()
        {
            return UnityEngine.Object.FindObjectOfType<MissionSceneLoader>();
        }
    }

    public static class MissionManagedDataFactory
    {
        public static void RefreshActionEntry(MissionStepActionEntry entry)
        {
            if (entry == null)
                return;
        }

        public static void RefreshConditionEntry(MissionStepConditionEntry entry)
        {
            if (entry == null)
                return;
        }
    }
}
