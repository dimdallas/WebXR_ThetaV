using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ThetaStreaming.Scripts
{
    public class ThetaVClientMode : MonoBehaviour {
        [Header("Theta Control")]
        public Material thetaMaterial;
        public bool useProxy = true;
        public bool MjpegDecode = true;
        private string executeCmd = "/osc/commands/execute";
        public string proxyUrl = "https://10.64.44.242:5000";
        public string thetaUrl = "http://10.64.45.228";
        public string thetaID = "THETAYL00245200";
        public string thetaPassword = "00245200";

        [Header("Live Preview Format")]
        public int texWidth = 1024;
        public int texHeight = 512;
        public int framerate = 30;

        private UnityWebRequest request;
        private string url;
        private bool succeeded = false;

        private void Awake()
        {
            if (thetaMaterial == null)
            {
                thetaMaterial = GetComponent<MeshRenderer>().material;
            }
        }

        private void Start()
        {
            if (useProxy)
                url = proxyUrl + executeCmd;
            else
                url = thetaUrl + executeCmd;
            
            StartCoroutine(GetLivePreview_UWR());
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            StopAllCoroutines();
            request.Abort();
        }

        // Use this for initialization and UnityWebRequest
        IEnumerator GetLivePreview_UWR ()
        {
            yield return StartCoroutine(SetOptions());

            if(MjpegDecode)
                yield return StartCoroutine(PreviewRequest());
            else
                yield return StartCoroutine(RepetitivePreviewRequests());
        }

        IEnumerator SetOptions()
        {
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
                certificateHandler = new HttpsCertificateHandler(),
                disposeUploadHandlerOnDispose = true,
                disposeDownloadHandlerOnDispose = true,
                disposeCertificateHandlerOnDispose = true
            };

            request.SetRequestHeader("Content-Type", "application/json");

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
                Debug.Log("Set options status: " + request.responseCode);
            }
            
            request.Dispose();
            request = null;
        }

        IEnumerator PreviewRequest()
        {
            byte[] byteBuffer = new byte[50000];
            byte[] postBytes = Encoding.Default.GetBytes("{\"name\" : \"camera.getLivePreview\"}");

            request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = (UploadHandler) new UploadHandlerRaw(postBytes),
                downloadHandler = (DownloadHandler) new MjpegDownloadHandler(byteBuffer, thetaMaterial),
                certificateHandler = new HttpsCertificateHandler(),
                disposeUploadHandlerOnDispose = true,
                disposeDownloadHandlerOnDispose = true,
                disposeCertificateHandlerOnDispose = true
            };
            // default for chunkedTransfer is false
            // request.chunkedTransfer = true;
            // request.useHttpContinue = true;

            request.SetRequestHeader("Content-Type", "application/json");

            //not yield return because runs endlessly
            UnityWebRequestAsyncOperation async =  request.SendWebRequest();
            // Debug.Log("below async");

            // yield return async;
            // Debug.Log("Returned from live request");
            
            
            if (request.isNetworkError)
            {
                Debug.Log("network error");
                Debug.Log(request.error);
            }
            else
            {
                // Debug.Log("not error");
                // Console.WriteLine("not error Console");
                // Debug.Log(request.downloadProgress);
                yield return null;
            }

            while (!async.isDone)
            {
                // Debug.Log("in while");
                // var jsonContent = request.downloadHandler.data;
                // Debug.Log("Coroutine " +request.downloadedBytes);
                // Debug.Log(request.downloadProgress);
                // if(request.GetResponseHeaders() != null)
                // {
                //     foreach (KeyValuePair<string, string> pair in request.GetResponseHeaders())
                //     {
                //         Debug.Log(pair.Key + ' ' + pair.Value);
                //     }
                // }
                // Debug.Log(async.progress);
                yield return new WaitForEndOfFrame();
            }
            
            request.Dispose();
            request = null;
            Debug.Log("Stream ended");
        }
        
        IEnumerator StartRecording()
        {
            //setOptions request
            byte[] postBytes = Encoding.Default.GetBytes("{\"name\" : \"camera.recordMjpeg\"}");
            // Debug.Log(System.Text.Encoding.Default.GetString(paramBytes));

            request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = (UploadHandler) new UploadHandlerRaw(postBytes),
                downloadHandler = (DownloadHandler) new DownloadHandlerBuffer(),
                certificateHandler = new HttpsCertificateHandler(),
                disposeUploadHandlerOnDispose = true,
                disposeDownloadHandlerOnDispose = true,
                disposeCertificateHandlerOnDispose = true
            };

            request.SetRequestHeader("Content-Type", "application/json");

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
                if (request.responseCode == 200)
                    succeeded = true;
                
                Debug.Log("StartRecording status: " + request.responseCode);
            }
            
            request.Dispose();
            request = null;
        }
        
        IEnumerator RepetitivePreviewRequests()
        {
            
            yield return StartCoroutine(StartRecording());
            
            byte[] byteBuffer = new byte[50000];
            byte[] postBytes = Encoding.Default.GetBytes("{\"name\" : \"camera.getJpeg\"}");
            
            while (true)
            {
                if (!succeeded)
                {
                    Debug.Log("Not Succeeded");
                    yield break;
                }

                request = new UnityWebRequest(url, "POST")
                {
                    uploadHandler = (UploadHandler) new UploadHandlerRaw(postBytes),
                    downloadHandler = (DownloadHandler) new SimpleDownloadHandler(byteBuffer, thetaMaterial),
                    certificateHandler = new HttpsCertificateHandler(),
                    disposeUploadHandlerOnDispose = true,
                    disposeDownloadHandlerOnDispose = true,
                    disposeCertificateHandlerOnDispose = true
                };
                // default for chunkedTransfer is false
                // request.chunkedTransfer = true;
                // request.useHttpContinue = true;

                request.SetRequestHeader("Content-Type", "application/json");
                
                //not yield return because runs endlessly
                UnityWebRequestAsyncOperation async =  request.SendWebRequest();
                // Debug.Log("below async");

                // yield return async;
                // Debug.Log("Returned from live request");
                
                
                if (request.isNetworkError)
                {
                    Debug.Log("network error");
                    Debug.Log(request.error);
                }
                else
                {
                    // Debug.Log("not error");
                    // Console.WriteLine("not error Console");
                    // Debug.Log(request.downloadProgress);
                    yield return null;
                }

                while (!async.isDone)
                {
                    // Debug.Log("in while");
                    // var jsonContent = request.downloadHandler.data;
                    // Debug.Log("Coroutine " +request.downloadedBytes);
                    // Debug.Log(request.downloadProgress);
                    // if(request.GetResponseHeaders() != null)
                    // {
                    //     foreach (KeyValuePair<string, string> pair in request.GetResponseHeaders())
                    //     {
                    //         Debug.Log(pair.Key + ' ' + pair.Value);
                    //     }
                    // }
                    // Debug.Log(async.progress);
                    yield return new WaitForEndOfFrame();
                }

                request.Dispose();
                request = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Debug.Log("Stream ended");
            }
            
        }
    }
}
