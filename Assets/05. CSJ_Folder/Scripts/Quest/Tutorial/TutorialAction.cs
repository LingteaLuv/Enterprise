using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class TutorialAction : MonoBehaviour
    {
        public void ShowHpBar()  => TutorialUtils.SetAlphaNow("HpBar", 1f, true, true);
        public void HideHpBar()  => TutorialUtils.SetAlphaNow("HpBar", 0f, false, false);

        public void SaveTeam() => TutorialUtils.BtnInvoke("SaveButton");
        public void PanelCloseBtn() => TutorialUtils.BtnInvoke("PanelCloseBtn");
        public void SkipPopUp() => TutorialUtils.BtnInvoke("leftButton");
    }
}