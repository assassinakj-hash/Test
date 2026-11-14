using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector3 direction;
    [SerializeField] private LayerMask wallMask;

    private Transform cachedTransform;

    private void Awake() => cachedTransform = transform;

    private void Update()
    {
        float step = speed * Time.deltaTime;

        if (Physics.Raycast(cachedTransform.position, direction, out RaycastHit hit, step, wallMask))
        {
            direction = Vector3.Reflect(direction, hit.normal);
            direction.y = 0f;
        } 

        cachedTransform.position += direction * step;
    }

    public void SetDirection(Vector3 newDirection) => direction = newDirection;
}
