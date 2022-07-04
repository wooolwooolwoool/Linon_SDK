using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Linq;

public class DistanceSensorBase : MonoBehaviour
{
    public List<DistanceSensor> objects;
    [System.NonSerialized] public List<float> distances = new List<float>();
    private int n;

    // Start is called before the first frame update
    void Start()
    {
        n = objects.Count;
        distances = Enumerable.Repeat<float>(0.0f, n).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        //string s = "";
        for (int i = 0; i < objects.Count; i++)
        {
            distances[i] = objects[i].distance;
            //s = s + " " + distances[i].ToString();
        }
        //Debug.Log(s);
    }
}
