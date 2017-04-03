using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/LostSight")]
public class LostSight : Decision {
    public override bool Decide(StateController controller) {
        bool result = controller.ValidateTarget();
        Debug.Log(result);
        return !result;
    }
}
