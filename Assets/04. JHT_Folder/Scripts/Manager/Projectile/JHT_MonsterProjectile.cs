using JHT;
using UnityEngine;

public class JHT_MonsterProjectile : JHT_PooledObject
{

    LayerMask targetLayer;
    protected float totalPower;
    [SerializeField] private Rigidbody2D rigid;

    public void Init(Vector2 _targetPos, Vector2 monsterPos,float projectileSpeed, float power)
    {
        gameObject.transform.position = monsterPos;
        totalPower = power;
        rigid.linearVelocity = (_targetPos - (Vector2)transform.position).normalized* projectileSpeed;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Crew"))
        {
            BaseCharecterFSM target = collision.GetComponent<BaseCharecterFSM>();

            if (target != null)
            {
                //Pool 파티클 사용
                target.TakeDamage(totalPower);
            }
            Release();
        }
    }
}
