using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ThetaStreaming.Scripts
{
    public class ThetaVClientMode : MonoBehaviour {
        [Header("Theta Control")]
        public Material thetaMaterial;
        public bool useProxy = true;
        private string executeCmd = "/osc/commands/execute";
        public string proxyUrl = "https://10.64.45.103:5000";
        public string thetaUrl = "http://10.64.45.228";
        public string thetaID = "THETAYL00245200";
        public string thetaPassword = "00245200";

        [Header("Live Preview Format")]
        public int texWidth = 1024;
        public int texHeight = 512;
        public int framerate = 30;

        private UnityWebRequest request;

        private void Awake()
        {
            if (thetaMaterial == null)
            {
                thetaMaterial = GetComponent<MeshRenderer>().material;
            }
        }

        private void Start()
        {
            StartCoroutine(GetLivePreview_UWR());
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            StopCoroutine(GetLivePreview_UWR());
            request.Abort();
        }

        // Use this for initialization and UnityWebRequest
        IEnumerator GetLivePreview_UWR ()
        {
            string url = "";
            if (useProxy)
                url = proxyUrl + executeCmd;
            else
                url = thetaUrl + executeCmd;

            //setOptions request
            byte[] setOptionsBytes = Encoding.Default.GetBytes("{"+
                                                          "\"name\":\"camera.setOptions\","+
                                                          "\"parameters\":{"+
                                                          "\"options\":{"+
                                                          "\"previewFormat\":{"+
                                                          "\"framerate\":"+framerate+","+
                                                          "\"height\":"+texHeight+","+
                                                          "\"width\":"+texWidth+"}}}}");
            // Debug.Log(System.Text.Encoding.Default.GetString(paramBytes));

            request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = (UploadHandler) new UploadHandlerRaw(setOptionsBytes),
                downloadHandler = (DownloadHandler) new DownloadHandlerBuffer(),
                certificateHandler = new HttpsBypass()
            };
            // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            request.SetRequestHeader("Content-Type", "application/json");
            // request.SetRequestHeader("Access-Control-Allow-Origin", "https://localhost/WebXR_theta/index.html");
            /*request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, Content-Type, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, PUT, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");*/
            
            //bypass HTTPS certificate
            // request.certificateHandler = new HttpsBypass();
            
            //Send the request then wait here until it returns
            yield return request.SendWebRequest();

            while (!request.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
            
            if (request.isNetworkError)
            {
                Debug.Log(request.error);
            }
            else
            {
                var jsonContent = request.downloadHandler.text;

                // Debug.Log(jsonContent);
            }
            
            
            //getLivePreview request
            byte[] byteBuffer = new byte[50000];
            byte[] postBytes = Encoding.Default.GetBytes("{\"name\" : \"camera.getLivePreview\"}");
            // Debug.Log(System.Text.Encoding.Default.GetString(postBytes));
            request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = (UploadHandler) new UploadHandlerRaw(postBytes),
                downloadHandler = (DownloadHandler) new MyDownloadHandler(byteBuffer,thetaMaterial),
                certificateHandler = new HttpsBypass()
            };
            // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            request.SetRequestHeader("Content-Type", "application/json");
            request.chunkedTransfer = false;
            
            //bypass HTTPS certificate
            // request.certificateHandler = new HttpsBypass();
            
            Debug.Log("Send live request");
            //not yield return because runs endlessly
            UnityWebRequestAsyncOperation async =  request.SendWebRequest();
            Debug.Log("below async");

            // yield return async;
            Debug.Log("Returned from live request");

            if (request.isNetworkError)
            {
                Debug.Log("network error");
                Debug.Log(request.error);
            }
            else
            {
                // Debug.Log("not error");
                // Console.WriteLine("not error Console");
                // Debug.Log(request.downloadHandler);
                yield return null;
            }

            while (!async.isDone)
            {
                // Debug.Log("in while");
                // var jsonContent = request.downloadHandler.data;
                // Debug.Log("Coroutine " +request.downloadedBytes);
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Stream ended");
        }
    }
}
