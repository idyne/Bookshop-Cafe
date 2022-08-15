using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class INACTIVE : State
{
    public INACTIVE(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override bool CanEnter()
    {
        return true;
    }

    public override void OnEnter()
    {
        stateMachine.gameObject.SetActive(false);
    }

    public override void OnExit()
    {

    }

    public override void OnTriggerEnter(Collider other)
    {

    }

    public override void OnTriggerExit(Collider other)
    {

    }

    public override void OnUpdate()
    {

    }
}