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

public class ControlManagerBase : MonoBehaviour
{
    [System.NonSerialized] public string message = "";
    public float time_limit;
    public int UDP_recv = 9000;
    public int UDP_send = 9001;
    public int TCP_recv = 9100;
    public bool show_dbg_messeage = false;

    protected UdpClient udpClientRecv, udpClientSend;
    protected TcpClient tcpClientSend;
    protected CancellationTokenSource cancellation = new CancellationTokenSource();
    protected TcpListener tcpServer;
    // info to send controler by UDP
    public static Dictionary<string, List<float>> currentinfo = new Dictionary<string, List<float>>();

    // Start is called before the first frame update
    void Start()
    {
        if (show_dbg_messeage)
            Debug.Log("start");
        udpClientRecv = new UdpClient(UDP_recv);
        udpClientRecv.BeginReceive(OnReceived, udpClientRecv);
        udpClientSend = new UdpClient();
        udpClientSend.Connect("127.0.0.1", UDP_send);

        System.Net.IPAddress localAddress =
               System.Net.IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(localAddress, TCP_recv);
        tcpServer = new TcpListener(localEndPoint);
        tcp_start();
        init_data();
    }

    // Initialize Method called at Start()
    public virtual void init_data(){}

    public async void tcp_start()
    {
        tcpServer.Start();
        cancellation.Token.Register(() => tcpServer.Stop());
        try
        {
            while (true)
            {
                if (show_dbg_messeage)
                    Debug.Log("waiting...");                  
                var tcpClient = await Task.Run(
                    () => tcpServer.AcceptTcpClientAsync(), cancellation.Token);
                if (show_dbg_messeage)
                    Debug.Log("connected");
                var request = await ReceiveAsync(tcpClient);
                if (show_dbg_messeage)
                    Debug.Log("finish connection");
            }
        }
        catch
        {
        }
        finally
        {
            if (tcpServer != null)
            {
                tcpServer.Stop();
                tcpServer = null;
            }
            if (show_dbg_messeage)
                Debug.Log("end");
        }
    }

    public async Task<string> ReceiveAsync(TcpClient tcpClient)
    {
        byte[] buffer = new byte[1024];
        string request = "";

        try
        {
            using (NetworkStream stream = tcpClient.GetStream())
            {
                do
                {
                    int byteSize = await stream.ReadAsync(buffer, 0, buffer.Length);
                    request += Encoding.UTF8.GetString(buffer, 0, byteSize);
                }
                while (stream.DataAvailable);
                if (show_dbg_messeage)
                    Debug.Log($"recive :{request}");
                OnEventRecv(request);
                var response = "OK";
                buffer = Encoding.ASCII.GetBytes(response);

                await stream.WriteAsync(buffer, 0, buffer.Length);
                if (show_dbg_messeage)
                    Debug.Log($"send {response}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        return request;
    }

    // Event process method called when event messeage is recived
    public virtual void OnEventRecv(string request){}

    private void OnReceived(System.IAsyncResult result)
    {
        UdpClient getUdp = (UdpClient)result.AsyncState;
        IPEndPoint ipEnd = null;
        byte[] getByte = getUdp.EndReceive(result, ref ipEnd);
        message = Encoding.UTF8.GetString(getByte);
        OnControal(message);
        getUdp.BeginReceive(OnReceived, getUdp);
    }

    // Controal method called when controal messeage is recived
    public virtual void OnControal(string message){}

    // method to prepare the infomation to send
    public virtual void GetInfo(){}

    // Update is called once per frame
    void FixedUpdate()
    {
        GetInfo();
        string json = DictToJSON<float>(currentinfo);
        if (show_dbg_messeage)
            Debug.Log(json);
        byte[] dgram = Encoding.UTF8.GetBytes(json);
        udpClientSend.Send(dgram, dgram.Length);
    }

    private void OnDestroy()
    {
        udpClientRecv.Close();
        udpClientSend.Close();
        cancellation.Cancel();
        if (tcpServer != null)
        {
            tcpServer.Stop();
            tcpServer = null;
        }
    }

    public string DictToJSON<T>(Dictionary<string, List<T>> dict, bool as_binary=false)
    {
        // [TODO] convert as binary
        string str = "{";
        foreach (var a in dict)
        {
            str = str + "\"" + a.Key + "\"";
            str = str + ":[";
            foreach (T aa in a.Value)
            {
                str = str + aa.ToString();
                str = str + ",";
            }
            str = str.Remove(str.Length - 1);
            str = str + "],";
        }
        str = str.Remove(str.Length - 1) + "}";
        return str;
    }
}
