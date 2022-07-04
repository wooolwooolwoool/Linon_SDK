using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PosReset : MonoBehaviour
{
    public List<GameObject> objects;
    static List<Vector3> poss = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject obj in objects)
        {
            Vector3 pos = obj.transform.position;
            if (pos == null)
            {
                Debug.Log("Obj is null!!");
            }
            poss.Add(obj.transform.position);
        }            
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        for (int i=0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            Vector3 pos = poss[i];

            Rigidbody rb = obj.transform.GetComponent<Rigidbody>();

            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.identity;
            rb.velocity = new Vector3(0, 0, 0);
            Debug.Log("Pos Reset");
        }
    }
}
