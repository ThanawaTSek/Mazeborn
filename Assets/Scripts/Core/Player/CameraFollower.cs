using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;
    private bool hasFoundPlayer = false;
    
    [Header("Camera")]
    [SerializeField] private float cameraSize = 2f;
    
    private void Start()
    {
        Camera.main.orthographicSize = cameraSize;
    }

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
            SmoothFollow();
        }
    }

    private void SmoothFollow()
    {
        // ตำแหน่งที่ต้องการของกล้อง
        Vector3 desiredPosition = target.position + offset;
        
        // ใช้ SmoothDamp เพื่อเคลื่อนที่แบบนุ่มนวล
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");
        if (player != null)
        {
            target = player.transform;
            hasFoundPlayer = true;
            Debug.Log("Camera is now following: " + player.name);
        }
    }
}
