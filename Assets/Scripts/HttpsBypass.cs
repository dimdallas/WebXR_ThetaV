using UnityEngine.Networking;

public class HttpsBypass : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        //Simply return true no matter what
        return true;
    }
}
