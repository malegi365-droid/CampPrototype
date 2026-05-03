using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCameraRebinder : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RebindNextFrame(scene.name));
    }

    private IEnumerator RebindNextFrame(string sceneName)
    {
        yield return null;
        yield return null;

        PartyControlManager party = FindFirstObjectByType<PartyControlManager>();
        CameraFollowProxy cameraProxy = FindFirstObjectByType<CameraFollowProxy>();

        if (party == null)
        {
            Debug.LogWarning("[SceneCameraRebinder] No PartyControlManager found after scene load.");
            yield break;
        }

        if (cameraProxy == null)
        {
            Debug.LogWarning("[SceneCameraRebinder] No CameraFollowProxy found after scene load.");
            yield break;
        }

        if (party.CurrentMember == null)
        {
            Debug.LogWarning("[SceneCameraRebinder] PartyControlManager has no CurrentMember.");
            yield break;
        }

        cameraProxy.SetTarget(party.CurrentMember.CameraFollowTarget, true);

        Debug.Log($"[SceneCameraRebinder] Rebound camera after loading scene: {sceneName}");
    }
}