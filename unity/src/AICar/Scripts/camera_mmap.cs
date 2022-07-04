using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.JsonUtility;
using System.IO.MemoryMappedFiles;
using System.IO;


public class camera_mmap : MonoBehaviour
{
    private Texture2D tex = null;

    private MemoryMappedFile mmf;
    private MemoryMappedViewAccessor accessor;

    private int w, h;

    [SerializeField] private float scale = 1;
    [SerializeField] private string mmap_name = "test";

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
        mmf = MemoryMappedFile.CreateNew(mmap_name, 1024 * 1024 * 50);
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnPostRender()
    {
        accessor = mmf.CreateViewAccessor();
        RenderTexture.active = cam.targetTexture;

        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        accessor.WriteArray<byte>(0, bytes, 0, bytes.Length);
        accessor.Dispose();
    }

    private void OnDestroy()
    {
        mmf.Dispose();
        accessor.Dispose();
        Destroy(tex);
        Destroy(cam.targetTexture);
    }
}
