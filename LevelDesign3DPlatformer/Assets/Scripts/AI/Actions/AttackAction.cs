using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Attack")]
public class AttackAction : Action {
    //TODO: Break apart into seperate pieces
    //      Allow for the action to run independantly of the animations
    //      Form a struct that drives the action

    public override void Act(StateController controller) {  
        if (!controller.AnimStateInfo.IsTag("Attack")) {
            Debug.Log("I am attacking D:<");
            controller.NavAgent.isStopped = true;
            controller.anim.SetTrigger("Attack");
        }
    }
}
