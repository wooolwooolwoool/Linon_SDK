using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceSensor : MonoBehaviour
{
    private const float NOTHING = -1;
    // Ray‚ð•\Ž¦‚·‚é‚©
    public bool drow_ray = false;
    // ‘ª’è‰Â”\”ÍˆÍ
    public float maxDistance = 30;
    [System.NonSerialized] public float distance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, fwd, out hit, maxDistance))
        {
            distance = hit.distance;
        }
        else
        {
            distance = NOTHING;
        }
        if (drow_ray)
        {
            Debug.DrawRay(transform.position, fwd * maxDistance, Color.red, 0, false);
        }
    }
}
