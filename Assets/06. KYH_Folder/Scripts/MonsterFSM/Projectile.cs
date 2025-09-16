using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 targetPosition;
    public float speed = 5f;

    private float damage; // 발사자가 지정한 데미지 저장

    public void Init(Vector3 targetPos, float damage)
    {
        targetPosition = targetPos;
        this.damage = damage;
        // TODO : 필요시 방향 계산이나 회전 등 추가
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Debug.Log("타겟에게 닿지않고 파괴됨");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Crew"))
        {
            var target = other.GetComponent<HealthSystem>();
            if (target != null)
            {
                //target.TakeDamage(damage); // 충돌 대상에게 데미지 전달
            }

            Debug.Log("Crew에게 명중되고 파괴됨.");
            Destroy(gameObject);
        }
    }
}
