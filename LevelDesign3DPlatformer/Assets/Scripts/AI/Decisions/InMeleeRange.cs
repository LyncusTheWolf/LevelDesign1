using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/InRange")]
public class InMeleeRange : Decision {
    public override bool Decide(StateController controller) {
        return Vector3.Distance(controller.target.transform.position, controller.transform.position) <= controller.characteristics.meleeRange;
    }
}
