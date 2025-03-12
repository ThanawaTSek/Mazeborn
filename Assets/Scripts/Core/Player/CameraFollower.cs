using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private bool hasFoundPlayer = false;

    private void Update()
    {
        if (!hasFoundPlayer)
        {
            TryFindPlayer();
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");
        if (player != null)
        {
            target = player.transform;
            hasFoundPlayer = true;
            Debug.Log("Camera now following: " + player.name);
        }
    }
}
