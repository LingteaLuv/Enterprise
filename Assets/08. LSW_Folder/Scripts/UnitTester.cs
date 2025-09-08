using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitTester : MonoBehaviour
{
    private Animator _animator;
    private TestController _testController;
    private Vector3 _targetPosition;
    private InputAction _clickAction;
    private InputAction _attackAction;

    private bool _isAttacking;
    private bool _isMoving;
    private readonly float _moveSpeed = 3f;
    private readonly float _attackCooldown = 1f;
    
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _testController = new TestController();
        _testController.Enable();
        _clickAction = _testController.Player.Click;
        _attackAction = _testController.Player.Attack;
        
        if (_animator == null)
        {
            Debug.Log("animator 설정 안됨");
        }

        if (_testController == null)
        {
            Debug.Log("testController 설정 안됨");
        }
    }
    
    private void OnEnable()
    {
        _testController.Enable();
        _clickAction.performed += ctx => Click();
        _attackAction.performed += ctx => Attack();
    }

    private void OnDisable()
    {
        _testController.Disable();
        _clickAction.performed -= ctx => Click();
        _attackAction.performed -= ctx => Attack();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_isMoving)
        {
            Debug.Log("_isMoving if문 호출");
            Vector3 direction = (_targetPosition - transform.position).normalized;
            transform.position += direction * (_moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                _isMoving = false;
                _animator.SetBool("1_Move", false);
            }
            else
            {
                _animator.SetBool("1_Move", true);
            }
        }
    }

    private void Click()
    {
        Debug.Log("Click 호출");
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));

        if (worldPos.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        
        _targetPosition = worldPos;
        _isMoving = true;
    }

    private void Attack()
    {
        if (_isAttacking) return;
        _isAttacking = true;
        _isMoving = false;
        _animator.SetBool("1_Move", false);
        _animator.SetTrigger("2_Attack");

        AttackCoolDownAsync().Forget();
    }

    private async UniTaskVoid AttackCoolDownAsync()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(_attackCooldown));
        _isAttacking = false;
    }
}
