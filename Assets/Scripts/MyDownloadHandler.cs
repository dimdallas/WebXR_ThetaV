using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MyDownloadHandler : DownloadHandlerScript
{
    private Material thetaMaterial;
    private Texture2D tex;
    private Texture2D tex2;
    // private bool TexStored = false;
    private List<byte> imageBytes;
    private bool isLoadStart = false;
    
    // Standard scripted download handler - will allocate memory on each ReceiveData callback
    public MyDownloadHandler(Material thetaMaterial)
        : base()
    {
        this.thetaMaterial = thetaMaterial;
        imageBytes= new List<byte>();
    }

    // Pre-allocated scripted download handler
    // Will reuse the supplied byte array to deliver data.
    // Eliminates memory allocation.
    public MyDownloadHandler(byte[] buffer, Material thetaMaterial)
        : base(buffer)
    {
        this.thetaMaterial = thetaMaterial;
        imageBytes = new List<byte>();
    }

    // Required by DownloadHandler base class. Called when you address the 'bytes' property.
    protected override byte[] GetData() { return null; }

    // Called once per frame when data has been received from the network.
    protected override bool ReceiveData(byte[] bytesFromCamera, int dataLength)
    {
        Debug.Log("CustomWebRequest :: ReceiveData - inside" + bytesFromCamera.Length + " " + dataLength);
        
        if (bytesFromCamera.Length < 1)
        {
            Debug.Log("CustomWebRequest :: ReceiveData - received a null/empty buffer");
            return false;
        }

        //Search of JPEG Image here
        for (int i = 0; i < dataLength; i ++)
        {
            // Debug.Log("CustomWebRequest :: ReceiveData - inside for");
            byte byteData1 = bytesFromCamera[i];
            

            if (!isLoadStart)
            {
                // mjpeg start! ( [0xFF 0xD8 ... )
                if (byteData1 == 0xFF)
                {
                    i++;
                    byte byteData2 = bytesFromCamera[i];
                    if(byteData2 == 0xD8)
                    {
                        imageBytes.Add(byteData1);
                        imageBytes.Add(byteData2);

                        isLoadStart = true;
                    }
                }
            }
            else
            {
                imageBytes.Add(byteData1);

                // mjpeg end (... 0xFF 0xD9] )
                if (byteData1 == 0xFF)
                {
                    if (i + 1 < dataLength)
                    {
                        i++;
                        byte byteData2 = bytesFromCamera[i];
                        imageBytes.Add(byteData2);
                        if (byteData2 == 0xD9)
                        {
                            tex = new Texture2D(2, 2);
                            tex.LoadImage((byte[]) imageBytes.ToArray());
                            thetaMaterial.mainTexture = tex;
                            imageBytes.Clear();
                            isLoadStart = false;
                        }
                    }
                }
            }
        }

        return true;
    }

    // Called when all data has been received from the server and delivered via ReceiveData
    protected override void CompleteContent()
    {
        Debug.Log("CustomWebRequest :: CompleteContent - DOWNLOAD COMPLETE!");
    }

    // Called when a Content-Length header is received from the server.
    [Obsolete("Use ReceiveContentLengthHeader")]
    protected override void ReceiveContentLength(int contentLength)
    {
        Debug.Log($"CustomWebRequest :: ReceiveContentLength - length {contentLength}");
    }
}
