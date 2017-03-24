using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumpPickup : SkillPickup {

	protected override void ModifyMotor(CharacterMotor motor) {
        motor.skills.ableToWallJump = true;
    }
}
