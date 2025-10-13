using JHT;
using System.Collections;
using UnityEngine;

public class JHT_MonsterProjectile : JHT_PooledObject
{
    // '누가 쐈는지' 저장할 변수
    private IAttacker owner;

    LayerMask targetLayer;
    protected float totalPower;
    [SerializeField] private SpriteRenderer projectileImg;
    [SerializeField] private Rigidbody2D rigid;

    Coroutine startCor;
    public void Init(IAttacker owner, Vector2 _targetPos, Vector2 monsterPos, Sprite baseMonsterSprite, float projectileSpeed)//, float power, )
    {
        // target이 체력 0이 돼도 같은 타겟에게 계속 날라가서 효과 적용함
        if (rigid == null)// || baseMonsterSprite == null)
        {
            Release();
            return;
        }

        this.owner = owner;
        gameObject.transform.position = monsterPos;
        //totalPower = power;
        rigid.linearVelocity = (_targetPos - (Vector2)transform.position).normalized * projectileSpeed;
        projectileImg.sprite = baseMonsterSprite;

        if ((_targetPos - (Vector2)gameObject.transform.position).magnitude > 0)
        {
            projectileImg.flipX = false;
        }
        else
            projectileImg.flipX = true;

        if (startCor == null)
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
            //IDamageable target = collision.GetComponent<IDamageable>();

            //if (target != null)
            //{
            //    //Pool 파티클 사용
            //    target.TakeDamage(this.owner, 1f);
            //}

            Release();

            if (startCor != null)
            {
                StopCoroutine(startCor);
                startCor = null;
            }
        }
    }
}
