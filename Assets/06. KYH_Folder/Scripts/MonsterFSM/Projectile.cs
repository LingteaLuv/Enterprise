using UnityEngine;

public class Projectile : MonoBehaviour
{
    // 임시로 투사체 스크립트 작업함 차후 피격시 사라지거나 하는 세부내용 구성예정.
    private Vector3 targetPosition;
    public float speed = 5f;

    public void Init(Vector3 targetPos)
    {
        targetPosition = targetPos;
        // TODO : 필요시 방향 계산이나 로직 추가
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // 명중 처리, 파괴 등
            Destroy(gameObject);

            Debug.Log("타겟에게 닿지않고 파괴됨");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Crew"))
        {
            Debug.Log(" Crew에게 명중되고 파괴됨.");

            // 데미지 처리
            Destroy(gameObject);
        }
    }
}
