using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/ForceTransition")]
public class ForceTransition : Decision {
    public override bool Decide(StateController controller) {
        return true;
    }
}
