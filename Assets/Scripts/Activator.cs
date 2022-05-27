using System;
using System.Collections.Generic;
using UnityEngine;

public class Activator {
    private float bias;
    private List<float> weights;

    private static Func<float[], float[], float, float> _inputFunction;
    private static Func<float, float> _activationFunction;

    public Activator(Func<float[], float[], float, float> inputFunction, Func<float, float> activationFunction) {
        //TODO: Procedural??
        weights = new List<float>();
        bias = 0;
        _inputFunction = inputFunction;
        _activationFunction = activationFunction;
    }

    public void addWeight(float val) {
        weights.Add(val);
    }

    public float calculateOutput(float[] inputs) {
        if (inputs.Length != weights.Count) {
            throw new InvalidOperationException("Weights length was not equal to inputs length");
        }
        float node = _inputFunction(inputs, weights.ToArray(), bias);
        return _activationFunction(node);
    }

    public static float linearInputFunction(float[] inputs, float[] weights, float bias) {
        float sum = 0;
        for (int i = 0; i < inputs.Length; i++) {
            sum += inputs[i] * weights[i];
        }

        sum += bias;

        return sum;
    }

    public static float sigmoidActivationFunction(float num) {
        return (float) (1 / (1 + Math.Pow(Math.E, -num)));
    }
    
}
