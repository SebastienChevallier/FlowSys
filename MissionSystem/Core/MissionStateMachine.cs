using UnityEngine;

namespace GAME.MissionSystem
{
    /// <summary>
    /// State Machine pour gérer les transitions entre états de mission
    /// Pattern: State
    /// </summary>
    public class MissionStateMachine
    {
        private IMissionState currentState;
        private IMissionContext context;
        
        public IMissionState CurrentState => currentState;
        
        public MissionStateMachine(IMissionContext context)
        {
            this.context = context;
        }
        
        public void ChangeState(IMissionState newState)
        {
            if (currentState != null)
            {
                currentState.OnExit(context);
            }
            
            currentState = newState;
            
            if (currentState != null)
            {
                currentState.OnEnter(context);
            }
        }
        
        public void Update()
        {
            currentState?.Update(context);
        }

        public void ClearStateWithoutExit()
        {
            currentState = null;
        }
    }
}
