using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Brain {
    public float speed = 0.005f;
    private List<NeuronGroup> _neuronGroups;

    public Brain() {
        _neuronGroups = new List<NeuronGroup>();
    }

    public void addNeuronGroup(List<Sense> senses, Action action) {
        NeuronGroup ng = new NeuronGroup(senses, action);
        _neuronGroups.Add(ng);
        
    }
    
    public List<Action> calculateActions() {
        List<Action> actionList = new List<Action>();
        
        foreach (NeuronGroup ng in _neuronGroups) {
            Action ngAction = ng.activate();
            if (ngAction is null) {
                continue;
            }
            actionList.Add(ngAction);
        }
        return actionList;
    }
}

class NeuronGroup {
    private List<Sense> _senses;
    private Action _action;
    private Activator _activator;

    public NeuronGroup(List<Sense> senses, Action action) {
        _senses = senses;
        _action = action;
        
        _activator = new Activator(Activator.linearInputFunction, Activator.sigmoidActivationFunction);
        foreach (Sense s in senses)
        {
            if (_action is RotateAction) {
                _activator.addWeight(-1);
            }
            else {
                _activator.addWeight(1);
            }
        }
    }

    public Action activate() {
        float[] readings = Sense.takeReadingInArray(_senses);
        float val = _activator.calculateOutput(readings);
        
        //TODO: How to do threshold?
        if (val >= 0.5) {
            return _action;
        }
        return null;
    }
    
    
}