using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 5f; // Kecepatan gerakan kamera
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f); // Jarak offset kamera (Z harus -10)

    private void LateUpdate()
    {
        // Memastikan PlayerController instance ada di scene agar tidak NullReferenceException
        if (PlayerController.instance != null)
        {
            // Menentukan posisi target tujuan kamera berdasarkan posisi Player + offset
            Vector3 targetPosition = PlayerController.instance.transform.position + offset;

            // Menggerakkan posisi kamera secara halus menggunakan Vector3.Lerp
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Menerapkan posisi baru ke Main Camera
            transform.position = smoothedPosition;
        }
    }
}