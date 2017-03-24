using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumpPickup : SkillPickup {

	protected override void ModifyMotor(CharacterMotor motor) {
        motor.skills.ableToDoubleJump = true;
    }
}
