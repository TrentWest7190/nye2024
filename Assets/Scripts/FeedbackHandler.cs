using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using System.Net;
using UnityEngine;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server.Extensions;
using MoreMountains.Feedbacks;
using Cinemachine;
using UnityEngine.SceneManagement;

public class FeedbackHandler : MonoBehaviour
{

    private GameObject ParseContext(HttpListenerContext context)
    {
        var jsonObject = context.Request.GetJsonBody<PostJsonBody>();

        if (jsonObject == null)
        {
            Debug.LogError("Empty Body");
            return null;
        }

        GameObject targetGO = GameObject.Find(jsonObject.Value.name);

        if (targetGO == null)
        {
            Debug.LogError("Cannot find GO " + jsonObject.Value.name);
            return null;
        }

        return targetGO;
    }

    [SimpleEventServerRouting(HttpConstants.MethodPost, "/runfeedback")]
    public void RunFeedback(HttpListenerContext context)
    {
        GameObject targetGO = ParseContext(context);

        targetGO.GetComponent<MMF_Player>().PlayFeedbacks();
    }

    [SimpleEventServerRouting(HttpConstants.MethodPost, "/stopfeedback")]
    public void StopFeedback(HttpListenerContext context)
    {
        GameObject targetGO = ParseContext(context);

        targetGO.GetComponent<MMF_Player>().StopFeedbacks();
    }

    [SimpleEventServerRouting(HttpConstants.MethodPost, "/stopandrestorefeedback")]
    public void StopAndRestoreFeedback(HttpListenerContext context)
    {
        GameObject targetGO = ParseContext(context);

        targetGO.GetComponent<MMF_Player>().StopFeedbacks();
        targetGO.GetComponent<MMF_Player>().RestoreInitialValues();
    }

    [SimpleEventServerRouting(HttpConstants.MethodGet, "/stopandrestoreall")]
    public void StopAndRestoreAll(HttpListenerContext context)
    {
        MMF_Player[] players = FindObjectsOfType<MMF_Player>();

        foreach (MMF_Player player in players)
        {
            player.StopFeedbacks();
            player.RestoreInitialValues();
        }

        context.Response.StatusCode = 200;
        context.Response.TextResponse("ok");
    }

    [SimpleEventServerRouting(HttpConstants.MethodPost, "/camera")]
    public void CutToCamera(HttpListenerContext context)
    {
        GameObject targetGO = ParseContext(context);

        if (targetGO == null)
        {
            return;
        }

        CinemachineVirtualCamera[] cams = FindObjectsOfType<CinemachineVirtualCamera>();

        foreach (CinemachineVirtualCamera cam in cams)
        {
            if (!cam.CompareTag("SpecialCamera"))
            {
                cam.enabled = false;
            }
            
            if (cam.gameObject.TryGetComponent<FlyCamera>(out var FC))
            {
                FC.enabled = false;
            }
        }

        targetGO.GetComponent<CinemachineVirtualCamera>().enabled = true;
        
        if (targetGO.TryGetComponent<FlyCamera>(out var flyCam))
        {
            flyCam.enabled = true;
        }
    }

    [SimpleEventServerRouting(HttpConstants.MethodGet, "/reload")]
    public void EmergencyReload()
    {
        // Get the active scene's name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Reload the scene by its name
        SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single);
    }

    public struct PostJsonBody
    {
        public string name;
    }
}
