using UnityEngine;
using UnityEngine.UI;

public class DemoSkill : MonoBehaviour
{
    [SerializeField] private Button demoSkillButton;

    [SerializeField] private GameObject[] skillEffectAnim;
    [SerializeField] private Transform target;

    private void Start()
    {
        demoSkillButton.onClick.AddListener(delegate { SetSkill(target); });
    }

    private void SetSkill(Transform target)
    {
        if (skillEffectAnim.Length > 1)
        {
            var obj1 = Instantiate(skillEffectAnim[0]);
            obj1.GetComponent<SkillEffectAnim>().Init(target);
            var obj2 = Instantiate(skillEffectAnim[1]);
            obj2.GetComponent<SkillEffectAnim>().Init(target);
        }
        else
        {
            var obj = Instantiate(skillEffectAnim[0]);
            obj.GetComponent<SkillEffectAnim>().Init(target);
        }
    }
}
