using UnityEngine;

/// <summary>
/// 오브젝트 풀에 의해 생성된 게임 오브젝트에 부착되는 도우미 컴포넌트입니다.
/// 자신이 어떤 원본 프리팹으로부터 생성되었는지에 대한 정보를 저장하여,
/// 풀에 다시 반납될 때 어떤 풀로 돌아가야 하는지 알려주는 역할을 합니다.
/// </summary>
public class PooledObject : MonoBehaviour
{
    // 이 오브젝트의 원본 프리팹 참조
    public GameObject Prefab { get; set; }
}
