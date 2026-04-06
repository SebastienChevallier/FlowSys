using System;
using System.Collections.Generic;
using UnityEditor;

namespace GAME.MissionSystem.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class MissionActionEditorRendererAttribute : Attribute
    {
        public Type ManagedType { get; }

        public MissionActionEditorRendererAttribute(Type managedType)
        {
            ManagedType = managedType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class MissionConditionEditorRendererAttribute : Attribute
    {
        public Type ManagedType { get; }

        public MissionConditionEditorRendererAttribute(Type managedType)
        {
            ManagedType = managedType;
        }
    }

    internal sealed class MissionInlineEditorContext
    {
        public Action SaveGraphPositions { get; set; }
        public Action Repaint { get; set; }
    }

    internal interface IMissionActionEditorRenderer
    {
        bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context);
    }

    internal interface IMissionConditionEditorRenderer
    {
        bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context);
    }

    internal static class MissionManagedTypeEditorRegistry
    {
        private static Dictionary<Type, IMissionActionEditorRenderer> actionRenderers;
        private static Dictionary<Type, IMissionConditionEditorRenderer> conditionRenderers;

        public static bool TryGetActionRenderer(Type managedType, out IMissionActionEditorRenderer renderer)
        {
            EnsureInitialized();
            if (managedType != null && actionRenderers.TryGetValue(managedType, out renderer))
                return true;

            renderer = null;
            return false;
        }

        public static bool TryGetConditionRenderer(Type managedType, out IMissionConditionEditorRenderer renderer)
        {
            EnsureInitialized();
            if (managedType != null && conditionRenderers.TryGetValue(managedType, out renderer))
                return true;

            renderer = null;
            return false;
        }

        private static void EnsureInitialized()
        {
            if (actionRenderers != null && conditionRenderers != null)
                return;

            actionRenderers = new Dictionary<Type, IMissionActionEditorRenderer>();
            conditionRenderers = new Dictionary<Type, IMissionConditionEditorRenderer>();

            foreach (Type type in TypeCache.GetTypesWithAttribute<MissionActionEditorRendererAttribute>())
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IMissionActionEditorRenderer).IsAssignableFrom(type))
                    continue;

                var attributes = (MissionActionEditorRendererAttribute[])Attribute.GetCustomAttributes(type, typeof(MissionActionEditorRendererAttribute));
                if (attributes == null || attributes.Length == 0)
                    continue;

                if (Activator.CreateInstance(type) is IMissionActionEditorRenderer renderer)
                {
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if (attributes[i]?.ManagedType != null)
                            actionRenderers[attributes[i].ManagedType] = renderer;
                    }
                }
            }

            foreach (Type type in TypeCache.GetTypesWithAttribute<MissionConditionEditorRendererAttribute>())
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IMissionConditionEditorRenderer).IsAssignableFrom(type))
                    continue;

                var attributes = (MissionConditionEditorRendererAttribute[])Attribute.GetCustomAttributes(type, typeof(MissionConditionEditorRendererAttribute));
                if (attributes == null || attributes.Length == 0)
                    continue;

                if (Activator.CreateInstance(type) is IMissionConditionEditorRenderer renderer)
                {
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if (attributes[i]?.ManagedType != null)
                            conditionRenderers[attributes[i].ManagedType] = renderer;
                    }
                }
            }
        }
    }
}
