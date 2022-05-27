using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPlacer : MonoBehaviour {
    public int NumberOfFood = 10;

    public MapParameters mps;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < NumberOfFood; i++) {
            GameObject food = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Renderer rend = food.GetComponent<Renderer>();
            rend.material.color = Color.green;
            
            
            Vector2 position = new Vector2(Random.Range(-mps.Width / 2, mps.Width / 2), Random.Range(- mps.Height / 2, mps.Height / 2));
            food.transform.position = position;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
