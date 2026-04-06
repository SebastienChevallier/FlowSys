namespace GAME.FlowSys
{
    /// <summary>
    /// Interface pour les états de mission (State Pattern)
    /// </summary>
    public interface IMissionState
    {
        void OnEnter(IMissionContext context);
        void Update(IMissionContext context);
        void OnExit(IMissionContext context);
    }
}
