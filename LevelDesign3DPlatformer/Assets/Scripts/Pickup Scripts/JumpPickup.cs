using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPickup : SkillPickup {

	protected override void ModifyMotor(CharacterMotor motor) {
        motor.skills.ableToJump = true;
    }
}
