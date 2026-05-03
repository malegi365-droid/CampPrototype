using UnityEngine;

public class DontDestroyOnLoadPlayer : MonoBehaviour
{
    private static bool alreadyExists = false;

    private void Awake()
    {
        if (alreadyExists)
        {
            Destroy(gameObject);
            return;
        }

        alreadyExists = true;
        DontDestroyOnLoad(gameObject);
    }
}