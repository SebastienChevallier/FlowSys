using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAME.MissionSystem
{
    [System.Serializable]
    public class UserActionToggle
    {
        public string actionId;
        public bool enabled;
    }

    [System.Serializable]
    public class MissionStepTransition
    {
        public string transitionId = "next";
        public string displayName = "Next";
        public MissionStepConfigSO targetStep;
    }

    [System.Serializable]
    public class MissionStepActionEntry
    {
        public MissionStepActionPhase phase = MissionStepActionPhase.OnEnter;
        [SerializeReference]
        public MissionActionData managedAction;
    }

    public enum MissionStepActionPhase
    {
        OnEnter,
        OnExit
    }

    [System.Serializable]
    public class MissionStepConditionEntry
    {
        [SerializeReference]
        public MissionConditionData managedCondition;

        [Tooltip("ID de la transition ciblée par cette condition. Si vide, utilise la première transition disponible.")]
        public string targetTransitionId = "";

        [Tooltip("ID de la transition secondaire utilisée par certaines conditions spéciales, comme Timer en cas de timeout.")]
        public string secondaryTargetTransitionId = "";
    }

    [CreateAssetMenu(fileName = "Step_New", menuName = "GAME/Mission System/Mission Step", order = 2)]
    public class MissionStepConfigSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("Step Information")]
        public string stepId;
        public string stepName;
        
        [Header("Editor")]
        [HideInInspector]
        public Vector2 editorPosition;

        [Header("Flow")]
        [Tooltip("Transitions sortantes de cette étape. Le runtime utilise pour l'instant la première transition valide.")]
        public List<MissionStepTransition> transitions = new List<MissionStepTransition>();

        [Header("Actions")]  
        [Tooltip("Actions ordonnées de l'étape")]
        public List<MissionStepActionEntry> actions = new List<MissionStepActionEntry>();
        
        [Header("Exit Conditions")]  
        [Tooltip("Conditions ordonnées de sortie d'étape")]
        public List<MissionStepConditionEntry> conditions = new List<MissionStepConditionEntry>();
        
        [Header("User Actions Configuration")]
        [Tooltip("UserActions à activer/désactiver pour cette étape")]
        public List<UserActionToggle> userActionToggles = new List<UserActionToggle>();

        public void OnBeforeSerialize()
        {
            EnsureDataIntegrity();            
        }

        public void OnAfterDeserialize()
        {
            EnsureLists();
        }

        private void OnValidate()
        {
            EnsureDataIntegrity();            
        }

        public void EnsureDataIntegrity()
        {
            EnsureLists();            
            EnsureManagedActionEntries();
            EnsureManagedConditionEntries();
            EnsureActionEntriesDefaults();
            EnsureConditionEntriesDefaults();
            EnsureTransitionEntriesConsistency();
        }

        public List<MissionStepActionEntry> GetStructuredActionEntries()
        {
            EnsureDataIntegrity();
            return actions;
        }

        public List<MissionStepActionEntry> GetStructuredActionEntries(MissionStepActionPhase phase)
        {
            EnsureDataIntegrity();
            List<MissionStepActionEntry> results = new List<MissionStepActionEntry>();
            for (int i = 0; i < actions.Count; i++)
            {
                MissionStepActionEntry entry = actions[i];
                if (entry != null && entry.phase == phase)
                    results.Add(entry);
            }

            return results;
        }

        public int GetStructuredActionCount(MissionStepActionPhase phase)
        {
            EnsureDataIntegrity();
            int count = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                MissionStepActionEntry entry = actions[i];
                if (entry != null && entry.phase == phase)
                    count++;
            }

            return count;
        }

        public List<MissionStepConditionEntry> GetStructuredConditionEntries()
        {
            EnsureDataIntegrity();
            return conditions;
        }

        public int GetStructuredConditionCount()
        {
            EnsureDataIntegrity();
            return conditions.Count;
        }

        public void AddStructuredAction(MissionStepActionEntry entry)
        {
            EnsureDataIntegrity();
            actions.Add(entry ?? new MissionStepActionEntry());
        }

        public void RemoveStructuredAction(MissionStepActionEntry entry)
        {
            EnsureDataIntegrity();
            if (entry == null)
                return;

            actions.Remove(entry);            
        }

        public void MoveStructuredAction(MissionStepActionEntry entry, int direction)
        {
            EnsureDataIntegrity();
            if (entry == null)
                return;

            List<MissionStepActionEntry> phaseEntries = GetStructuredActionEntries(entry.phase);
            int phaseIndex = phaseEntries.IndexOf(entry);
            int targetPhaseIndex = phaseIndex + direction;
            if (phaseIndex < 0 || targetPhaseIndex < 0 || targetPhaseIndex >= phaseEntries.Count)
                return;

            MissionStepActionEntry target = phaseEntries[targetPhaseIndex];
            int sourceIndex = actions.IndexOf(entry);
            int targetIndex = actions.IndexOf(target);
            if (sourceIndex < 0 || targetIndex < 0)
                return;

            actions[sourceIndex] = target;
            actions[targetIndex] = entry;
            
        }

        public void AddStructuredCondition(MissionStepConditionEntry entry)
        {
            EnsureDataIntegrity();
            conditions.Add(entry ?? new MissionStepConditionEntry());
            
        }

        public void RemoveStructuredCondition(MissionStepConditionEntry entry)
        {
            EnsureDataIntegrity();
            if (entry == null)
                return;

            conditions.Remove(entry);
            
        }

        public void MoveStructuredCondition(MissionStepConditionEntry entry, int direction)
        {
            EnsureDataIntegrity();
            if (entry == null)
                return;

            int sourceIndex = conditions.IndexOf(entry);
            int targetIndex = sourceIndex + direction;
            if (sourceIndex < 0 || targetIndex < 0 || targetIndex >= conditions.Count)
                return;

            MissionStepConditionEntry target = conditions[targetIndex];
            conditions[sourceIndex] = target;
            conditions[targetIndex] = entry;
            
        }

       

        private void EnsureLists()
        {
            transitions ??= new List<MissionStepTransition>();            
            actions ??= new List<MissionStepActionEntry>();            
            conditions ??= new List<MissionStepConditionEntry>();
            userActionToggles ??= new List<UserActionToggle>();
        }

        
        

        

        private void EnsureActionEntriesDefaults()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                MissionStepActionEntry entry = actions[i];
                if (entry == null)
                {
                    actions[i] = new MissionStepActionEntry();
                    continue;
                }
            }
        }

        private void EnsureManagedActionEntries()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                MissionStepActionEntry entry = actions[i];
                if (entry == null)
                    continue;

                global::GAME.MissionSystem.MissionManagedDataFactory.RefreshActionEntry(entry);
            }
        }

        private void EnsureManagedConditionEntries()
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                MissionStepConditionEntry entry = conditions[i];
                if (entry == null)
                    continue;

                global::GAME.MissionSystem.MissionManagedDataFactory.RefreshConditionEntry(entry);
            }
        }

        private void EnsureConditionEntriesDefaults()
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                MissionStepConditionEntry entry = conditions[i];
                if (entry == null)
                {
                    conditions[i] = new MissionStepConditionEntry();
                    continue;
                }
            }
        }

        private void EnsureTransitionEntriesConsistency()
        {
            if (transitions == null)
                transitions = new List<MissionStepTransition>();

            HashSet<string> validTransitionIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < conditions.Count; i++)
            {
                MissionStepConditionEntry entry = conditions[i];
                if (entry == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(entry.targetTransitionId))
                    validTransitionIds.Add(entry.targetTransitionId);

                if (!string.IsNullOrWhiteSpace(entry.secondaryTargetTransitionId))
                    validTransitionIds.Add(entry.secondaryTargetTransitionId);
            }

            Dictionary<string, MissionStepTransition> transitionsById = new Dictionary<string, MissionStepTransition>(StringComparer.Ordinal);
            List<MissionStepTransition> normalizedTransitions = new List<MissionStepTransition>();
            for (int i = 0; i < transitions.Count; i++)
            {
                MissionStepTransition transition = transitions[i];
                if (transition == null || string.IsNullOrWhiteSpace(transition.transitionId))
                    continue;

                if (!validTransitionIds.Contains(transition.transitionId))
                    continue;

                if (transitionsById.ContainsKey(transition.transitionId))
                    continue;

                transitionsById.Add(transition.transitionId, transition);
                normalizedTransitions.Add(transition);
            }

            transitions = normalizedTransitions;

            for (int i = 0; i < conditions.Count; i++)
            {
                MissionStepConditionEntry entry = conditions[i];
                if (entry == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(entry.targetTransitionId) && !transitionsById.ContainsKey(entry.targetTransitionId))
                    entry.targetTransitionId = string.Empty;

                if (!string.IsNullOrWhiteSpace(entry.secondaryTargetTransitionId) && !transitionsById.ContainsKey(entry.secondaryTargetTransitionId))
                    entry.secondaryTargetTransitionId = string.Empty;
            }
        }
    }
}
