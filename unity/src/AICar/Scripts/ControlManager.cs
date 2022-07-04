using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO.MemoryMappedFiles;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Linq;
using static System.Console;

public class Operate
{
    public float mo;
    public float st;
}

public class ControlManager : ControlManagerBase
{
    // Car to control
    public Car car;
    public DistanceSensorBase dsb;
    public PosReset posreset;
    public StopwatchScript stopwatch;
    protected Rigidbody carrigid;

    public override void init_data()
    {
        carrigid = car.GetComponent<Rigidbody>();
        currentinfo.Add("X", new List<float>() { 0.0f });
        currentinfo.Add("Y", new List<float>() { 0.0f });
        currentinfo.Add("Z", new List<float>() { 0.0f });
        currentinfo.Add("V", new List<float>() { 0.0f });
        currentinfo.Add("time", new List<float>() { 0.0f });
        currentinfo.Add("distances", Enumerable.Repeat<float>(0.0f, dsb.objects.Count).ToList());
    }

    public override void OnEventRecv(string request)
    {
        if (request == "reset")
        {
            stopwatch.OnClick2();
            posreset.OnClick();
        }
    }

    public override void OnControal(string message){
        if (show_dbg_messeage)
            Debug.Log(message);
        Operate op = JsonUtility.FromJson<Operate>(message);
        car.remoteMotorTorque = op.mo;
        car.remoteSteeringAngle = op.st;
    }

    public override void GetInfo()
    {
        Vector3 pos = car.transform.position;
        currentinfo["X"][0] = pos.x;
        currentinfo["Y"][0] = pos.y;
        currentinfo["Z"][0] = pos.z;
        currentinfo["V"][0] = carrigid.velocity.magnitude;
        currentinfo["time"][0] = get_time();

        for (int i = 0; i < dsb.distances.Count; i++)
        {
            currentinfo["distances"][i] = dsb.distances[i];
        }
    }
    
    public float get_time()
    {
        if (stopwatch.a != 0)
        {
            return  stopwatch.countTime;
        }
        else if (stopwatch.countTime > time_limit)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}