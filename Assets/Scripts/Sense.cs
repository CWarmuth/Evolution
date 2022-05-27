using System.Collections.Generic;
using UnityEngine;

public abstract class Sense {
    public abstract float takeReading();

    public static float[] takeReadingInArray(List<Sense> senses) {
        float[] arr = new float[senses.Count];
        for (int i = 0; i < senses.Count; i++) {
            arr[i] = senses[i].takeReading();
        }

        return arr;
    }
}

public class Eyes : Sense {
    private float _sightDistance;
    private Transform _transform;

    public Eyes(Transform transform, float sightDistance) {
        _sightDistance = sightDistance;
        _transform = transform;
    }
    
    public override float takeReading() {
        RaycastHit hit;
        bool result = Physics.Raycast(_transform.position, _transform.up, out hit, _sightDistance);
        
        if (result) {
            Debug.DrawRay(_transform.position, _transform.up * hit.distance, Color.yellow);
        } else {
            Debug.DrawRay(_transform.position, _transform.up * 1000, Color.white);
        }
        
        return result ? 1f : 0f;
    }
}