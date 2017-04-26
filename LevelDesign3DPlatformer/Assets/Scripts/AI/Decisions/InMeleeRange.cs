using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/InRange")]
public class InMeleeRange : Decision {
    public override bool Decide(StateController controller) {
        float angleBetween = Vector3.Angle(controller.transform.forward, controller.target.transform.position - controller.transform.position);

        return Vector3.Distance(controller.target.transform.position, controller.transform.position) <= controller.characteristics.meleeRange && angleBetween < controller.characteristics.minAttackAngle;
    }
}
