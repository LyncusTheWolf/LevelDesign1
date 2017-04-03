using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/ChasePlayer")]
public class ChaseAction : Action {
    public override void Act(StateController controller) {
        controller.NavAgent.destination = controller.target.transform.position;
        controller.NavAgent.Resume();
    }
}
