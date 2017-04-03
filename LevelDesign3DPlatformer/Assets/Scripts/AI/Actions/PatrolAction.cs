using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Patrol")]
public class PatrolAction : Action {
    public override void Act(StateController controller) {
        controller.NavAgent.destination = controller.wayPointList[controller.currentWayPoint].position;
        controller.NavAgent.Resume();

        if(controller.NavAgent.remainingDistance <= controller.NavAgent.stoppingDistance && !controller.NavAgent.pathPending) {
            controller.currentWayPoint = (controller.currentWayPoint + 1) % controller.wayPointList.Count;
        }
    }
}
