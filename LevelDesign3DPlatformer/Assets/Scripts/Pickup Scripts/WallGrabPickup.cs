using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGrabPickup : SkillPickup {

	protected override void ModifyMotor(CharacterMotor motor) {
        motor.skills.ableToWallGrab = true;
    }
}
