using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
    private Brain brain;

    private Eyes eyes;
    // Start is called before the first frame update
    void Start()
    {
        brain = new Brain();
        
        
        List<Sense> senses = new List<Sense>();
        eyes = new Eyes(transform, 100f);
        senses.Add(eyes);
        
        Action firstAction = new ForwardMovementAction(0.005f);
        Action secondAction = new RotateAction(0.05f);
        
        brain.addNeuronGroup(senses, firstAction);
        brain.addNeuronGroup(senses, secondAction);
       
    }

    // Update is called once per frame
    void Update() {
        List<Action> actions = brain.calculateActions();
        
        foreach(Action a in actions)
        {
            a.applyAction(transform);
        }
    }
}
