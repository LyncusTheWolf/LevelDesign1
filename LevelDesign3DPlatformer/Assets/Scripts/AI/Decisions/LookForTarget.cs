using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Append this so that it works generically
[CreateAssetMenu(menuName = "PluggableAI/Decisions/LookForPlayer")]
public class LookForTarget : Decision {
    public override bool Decide(StateController controller) {
        Character targetFind = null;
        if (controller.PollObjectsInSight(ref targetFind)) {
            controller.target = targetFind;
            return true;
        }

        return false;
    }
}
