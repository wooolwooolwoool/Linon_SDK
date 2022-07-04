using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalLineSc : MonoBehaviour
{
    public GameObject obj;
    StopwatchScript script;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name == "Body")
        {
            obj = GameObject.Find("Stopwatch");
            script = obj.GetComponent<StopwatchScript>();
            if (script.a == 0)
            {
                script.a = 1;
            }
            else
            {
                script.a = 0;
            }
        }
    }
}
