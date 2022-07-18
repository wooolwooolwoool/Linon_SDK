using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMove : MonoBehaviour
{
    public float speed = 1.0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= transform.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0,speed * Time.deltaTime * 5,0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0,-1*speed * Time.deltaTime * 5,0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * speed * Time.deltaTime;
        }

    }
}