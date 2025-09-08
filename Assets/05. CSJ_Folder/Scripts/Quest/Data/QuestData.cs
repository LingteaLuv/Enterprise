using System;
using System.Collections.Generic;

namespace _05._CSJ_Folder.Scripts.Quest.Data
{
    [Serializable]
    public class QuestData
    {
        public GeneralQuestData General;
        public Dictionary<string,TemporaryQuestData> Temporary;
    }
}