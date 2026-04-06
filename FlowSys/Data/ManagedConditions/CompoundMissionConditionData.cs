using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAME.FlowSys
{
    [System.Flags]
    public enum LogicalOperator
    {
        AND = 0,
        OR = 1,
        XOR = 2
    }

    [MissionActionCategory("Flow")]
    [Serializable]
    public sealed class CompoundMissionConditionData : MissionConditionData
    {
        [SerializeReference]
        public List<MissionStepConditionEntry> SubConditions = new List<MissionStepConditionEntry>();

        public LogicalOperator Operator = LogicalOperator.AND;

        [SerializeField]
        public bool WaitForParentStepOnEnterCompletion = false;

        public override string GetDisplayName()
        {
            string operatorStr = Operator.ToString();
            int count = SubConditions != null ? SubConditions.Count : 0;
            return $"Compound Condition ({operatorStr}: {count} conditions)";
        }

        public override string GetTypeName()
        {
            return nameof(CompoundMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            // Si on attend la complétude des actions OnEnter, bloquer tant qu'elles ne sont pas complétées
            if (WaitForParentStepOnEnterCompletion && !context.AreStepOnEnterActionsComplete())
                return false;

            // Évaluer TOUTES les sous-conditions (pas de court-circuit)
            if (SubConditions == null || SubConditions.Count == 0)
                return true; // Aucune sous-condition = considérée comme vraie

            var results = new List<bool>();
            foreach (var entry in SubConditions)
            {
                if (entry == null || entry.managedCondition == null)
                {
                    results.Add(true);
                    continue;
                }

                bool result = entry.managedCondition.Evaluate(context);
                results.Add(result);
            }

            // Combiner les résultats selon l'opérateur
            return Operator switch
            {
                LogicalOperator.AND => results.All(r => r),
                LogicalOperator.OR => results.Any(r => r),
                LogicalOperator.XOR => results.Count(r => r) == 1,
                _ => false
            };
        }
    }
}
