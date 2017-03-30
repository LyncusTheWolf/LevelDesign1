using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrengthSkillup : SkillPickup {

    protected override void ModifyMotor(CharacterMotor motor) {
        motor.skills.ableToPushBlocks = true;
    }
}
