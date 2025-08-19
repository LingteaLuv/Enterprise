using UnityEngine;

namespace JHT
{
    [CreateAssetMenu(menuName = "Scriptable_Weapon", fileName = "Scriptable_Weapon/Weapon")]
    public class Weapon : ItemWeapon
    {
        [field: SerializeField] public GameObject projectile { get; private set; } = null;
    }
}
