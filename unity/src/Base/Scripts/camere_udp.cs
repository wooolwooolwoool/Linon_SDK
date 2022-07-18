using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Linq;

public class camere_udp : MonoBehaviour
{
    private Texture2D tex = null;

    protected UdpClient udpClientSend;
    private int w, h;

    [SerializeField] private float scale = 1;
    [SerializeField] private int port = 10000;
    [SerializeField] private int quality = 50;
    [SerializeField] private string ip_add = "120.0.0.1";

    private GameObject mainCamObj;
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        mainCamObj = this.gameObject;
        cam = mainCamObj.GetComponent<Camera>();
        w = (int)((float)cam.pixelWidth * scale);
        h = (int)((float)cam.pixelHeight * scale);
        Debug.Log(w);
        Debug.Log(h);
        tex = new Texture2D(w, h, TextureFormat.ARGB32, false);

        if (cam.targetTexture != null)
            cam.targetTexture.Release();

        cam.targetTexture = new RenderTexture(w, h, 24);

        udpClientSend = new UdpClient();
        udpClientSend.Connect(ip_add, port);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        RenderTexture.active = cam.targetTexture;

        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToJPG(quality);
        udpClientSend.Send(bytes, bytes.Length);
        Debug.Log(bytes.Length);
    }

    private void OnDestroy()
    {
        udpClientSend.Close();
    }
}
