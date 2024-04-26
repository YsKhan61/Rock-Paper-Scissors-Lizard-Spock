using RPSLS.Services.Base;
using RPSLS.StateMachine;
using RPSLS.StateMachine.States;

namespace RPSLS.Services
{
    public class StateMachineService : ServiceBase
    {
        /// <summary>
        /// Get the current state machine.
        /// </summary>
        internal GameplaySystem CurrentFsm { get; private set; }

        protected override void Awake()
        {
            CreateSystem();
            base.Awake();
        }

        protected override void RegisterService()
        {
            Bootstrap.RegisterService(this);
        }

        private void CreateSystem()
        {
            CurrentFsm ??= new GameplaySystem(this);
            CurrentFsm.SetState(new InitialState());
        }
    }
}