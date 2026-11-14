using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform player;

    private Transform cachedTransform;

    private void Awake() => cachedTransform = transform;

    private void Update() => cachedTransform.position = Vector3.Lerp(cachedTransform.position, player.position, Time.deltaTime * speed);

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Bullet bullet)) return;

        Vector3 toPlayer = player.position - bullet.transform.position;
        toPlayer.y = 0f;
        bullet.SetDirection(toPlayer.normalized);

        Destroy(gameObject);
    }
}
