using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform posA; // Titik Bawah / Awal
    [SerializeField] private Transform posB; // Titik Atas / Tujuan
    [SerializeField] private float speed = 3f; // Kecepatan gerak lift
    [SerializeField] private float waitTime = 1.5f; // Jeda waktu (detik) saat sampai di ujung

    private Vector3 targetPos;
    private bool isWaiting = false;

    private void Start()
    {
        if (posA != null && posB != null)
        {
            transform.position = posA.position;
            targetPos = posB.position;
        }
    }

    private void Update()
    {
        if (isWaiting || posA == null || posB == null) return;

        // Gerakkan platform secara konstan menuju targetPos
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Jika platform sudah sampai di titik tujuan
        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            StartCoroutine(WaitAndSwitchTarget());
        }
    }

    private IEnumerator WaitAndSwitchTarget()
    {
        isWaiting = true;

        // Tunggu sesuai waktu jeda yang diset
        yield return new WaitForSeconds(waitTime);

        // Tukar tujuan (jika dari A ke B, ubah jadi B ke A)
        targetPos = (targetPos == posA.position) ? posB.position : posA.position;

        isWaiting = false;
    }

    // Agar Player ikut terbawa naik-turun saat berdiri di atas lift
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Cek jika Player mendarat dari atas platform
            if (collision.transform.position.y > transform.position.y)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Lepaskan Player dari ikatan platform saat melompat / turun
            collision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmos()
    {
        // Menampilkan garis bantu berwarna cyan di jendela Scene
        if (posA != null && posB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(posA.position, posB.position);
            Gizmos.DrawWireCube(posA.position, transform.localScale);
            Gizmos.DrawWireCube(posB.position, transform.localScale);
        }
    }
}