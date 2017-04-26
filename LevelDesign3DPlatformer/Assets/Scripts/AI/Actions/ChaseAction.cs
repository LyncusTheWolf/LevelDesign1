using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/ChasePlayer")]
public class ChaseAction : Action {
    public override void Act(StateController controller) {
        if (!controller.NavAgent.hasPath) {
            Quaternion lookRotTarget = Quaternion.LookRotation(controller.target.transform.position - controller.transform.position, Vector3.up);
            controller.target.transform.rotation = Quaternion.Lerp(controller.target.transform.rotation, lookRotTarget, controller.NavAgent.angularSpeed * Time.deltaTime);
        }

        controller.NavAgent.destination = controller.target.transform.position;
        controller.NavAgent.Resume();
        //controller.NavAgent.SetDestination(controller.target.transform.position);
    }
}
