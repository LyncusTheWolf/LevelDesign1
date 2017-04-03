using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/State")]
public class State : ScriptableObject{

    public float minimumTimeInState = 0.0f;

    public Action[] actions;
    public Transition[] transitions;

    public void UpdateState(StateController controller) {
        DoActions(controller);
        if (controller.CurrentStateTimer > minimumTimeInState) {
            CheckTransitions(controller);
        }
    }

    public void DoActions(StateController controller) {
        for (int i = 0; i < actions.Length; i++) {
            actions[i].Act(controller);
        }
    }

    private void CheckTransitions(StateController controller) {
        for (int i = 0; i < transitions.Length; i++) {
            if (Random.Range(0.0f, 100.0f) < transitions[i].chance) {
                if (transitions[i].decision.Decide(controller)) {
                    controller.TransitionToState(transitions[i].newState);
                    return;
                }
            }
        }
    }
}
