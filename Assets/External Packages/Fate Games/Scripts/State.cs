using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    [System.Serializable]
    public abstract class State
    {
        protected string name;
        protected StateMachine stateMachine;

        public string Name { get => name; }

        public State(StateMachine stateMachine)
        {
            name = GetType().ToString();
            this.stateMachine = stateMachine;
        }

        public abstract bool CanEnter();
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
        public abstract void OnTriggerEnter(Collider other);
        public abstract void OnTriggerExit(Collider other);

    }

}
