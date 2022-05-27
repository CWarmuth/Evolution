using Unity.VisualScripting;
using UnityEngine;

public abstract class Action {
    public abstract void applyAction(Transform transform);

}

public class RotateAction : Action {

    private float rot;

    public RotateAction(float rotation) {
        rot = rotation;
    }
    
    public override void applyAction(Transform transform) {
        transform.Rotate(0, 0, rot);
    }
}

public class MovementAction : Action {
    private Vector2 mov;
    private float speed;

    public MovementAction(float speed) {
        this.speed = speed;
    }

    public MovementAction(Vector2 movement) {
        mov = movement;
    }

    public void setDirection(Vector2 dir) {
        mov = dir;
    }
    
    public override void applyAction(Transform transform) {
        transform.Translate(mov * speed);
    }
}

public class ForwardMovementAction : MovementAction {
    public ForwardMovementAction(float speed) : base(speed) {
        setDirection(Vector2.up);
    }
}