using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleDownloadHandler : DownloadHandlerScript
{
    private Texture2D tex;
    private Material thetaMaterial;
    private List<byte> dataStream;

    // Standard scripted download handler - will allocate memory on each ReceiveData callback
    public SimpleDownloadHandler(Material thetaMaterial)
        : base()
    {
        this.thetaMaterial = thetaMaterial;
        dataStream = new List<byte>();
    }

    // Pre-allocated scripted download handler
    // Will reuse the supplied byte array to deliver data.
    // Eliminates memory allocation.
    public SimpleDownloadHandler(byte[] buffer, Material thetaMaterial)
        : base(buffer)
    {
        tex = new Texture2D(2, 2);
        this.thetaMaterial = thetaMaterial;
        dataStream = new List<byte>();
    }

    // Required by DownloadHandler base class. Called when you address the 'bytes' property. ('data' property)
    protected override byte[] GetData() { return null; }

    // Called once per frame when data has been received from the network.
    protected override bool ReceiveData(byte[] bytesFromCamera, int dataLength)
    {
        // Debug.Log("SimpleDownloadHandler :: ReceiveData " + dataLength);
        
        if (bytesFromCamera.Length < 1)
        {
            Debug.Log("CustomWebRequest :: ReceiveData - received a null/empty buffer");
            return false;
        }
        
        //Parse JPEG Image to material
        //JPEG ~35000 at 640x320
        if (dataLength < 35000)
        {
            dataStream.AddRange(bytesFromCamera);
            if (dataStream.Count > 35000)
            {
                Debug.Log("SimpleDownloadHandler :: Concatenate bytes " + dataStream.Count);
                tex.LoadImage((byte[]) dataStream.ToArray());
                thetaMaterial.mainTexture = tex;
                dataStream.Clear();
            }
        }
        else
        {
            tex.LoadImage(bytesFromCamera);
            thetaMaterial.mainTexture = tex;
        }
        
        // Debug.Log("image bytes " +imageBytes.Count);
        // Debug.Log("End");
        return true;
    }

    // Called when all data has been received from the server and delivered via ReceiveData
    protected override void CompleteContent()
    {
        // Debug.Log("CustomWebRequest :: CompleteContent - DOWNLOAD COMPLETE!");
    }

    // Called when a Content-Length header is received from the server.
    protected override void ReceiveContentLengthHeader(ulong contentLength)
    {
        // Debug.Log("My ReceiveContentLengthHeader");
        // Debug.Log($"CustomWebRequest :: ReceiveContentLengthHeader - length {contentLength}");
    }
    
    [Obsolete("Use ReceiveContentLengthHeader")]
    protected override void ReceiveContentLength(int contentLength)
    {
        // Debug.Log("My ReceiveContentLength");
        // Debug.Log($"CustomWebRequest :: ReceiveContentLength - length {contentLength}");
    }
}
