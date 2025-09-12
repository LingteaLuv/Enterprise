using JHT;
using System.Collections;
using UnityEngine;

public class JHT_MonsterProjectile : JHT_PooledObject
{

    LayerMask targetLayer;
    protected float totalPower;
    [SerializeField] private SpriteRenderer projectileImg;
    [SerializeField] private Rigidbody2D rigid;

    Coroutine startCor;
    public void Init(Vector2 _targetPos, Vector2 monsterPos,float projectileSpeed, float power, Sprite baseMonsterSprite)
    {
        if (baseMonsterSprite == null || rigid == null)
        {
            Release();
            return;
        }

        gameObject.transform.position = monsterPos;
        totalPower = power;
        rigid.linearVelocity = (_targetPos - (Vector2)transform.position).normalized * projectileSpeed;
        projectileImg.sprite = baseMonsterSprite;

        if(startCor == null)
        {
            StartCoroutine(CountForLimit());
        }
    }

    IEnumerator CountForLimit()
    {
        yield return new WaitForSeconds(3f);
        if (gameObject.activeSelf)
        {
            Release();

            if (startCor != null)
            {
                StopCoroutine(startCor);
                startCor = null;
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Crew"))
        {
            HealthSystem target = collision.GetComponent<HealthSystem>();

            if (target != null)
            {
                //Pool 파티클 사용
                target.TakeDamage(totalPower);
            }

            Release();

            if (startCor != null)
            {
                StopCoroutine(startCor);
                startCor = null;
            }
        }
    }
}
