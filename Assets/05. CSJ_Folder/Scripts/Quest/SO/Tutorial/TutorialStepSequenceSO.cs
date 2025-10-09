using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.SO.Tutorial
{
    [CreateAssetMenu(menuName = "Tutorial/StepSequence")]
    public class TutorialStepSequenceSO : ScriptableObject
    {
        public TutorialStepSO[] steps;
    }
}