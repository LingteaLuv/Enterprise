using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Codex.UI
{
    public class CodexListController : MonoBehaviour
    {
        [SerializeField] private RectTransform _transform;
        [SerializeField] private CodexPrefab _prefab;
        
        public Dictionary<(Faction, CodexStd_Enum),List<CodexInstance>> _instMap;
        private Dictionary<CodexInstance, CodexPrefab> _instanceMap;
        
        
        private void Awake()
        {
            _instanceMap = new Dictionary<CodexInstance, CodexPrefab>();
        }
        public void RebuildList(Faction faction, CodexStd_Enum codexType)
        {
            for (int i = _transform.childCount - 1; i >= 0; i--)
                Destroy(_transform.GetChild(i).gameObject);
            if (_instanceMap == null) return;
            _instanceMap.Clear();
            
            bool isFirst = true;
            foreach (var inst in _instMap[(faction, codexType)])
            {
                if (inst.IsReceived) continue;
                var card = Instantiate(_prefab, _transform);
                if (isFirst)
                {
                    card.isFirst = true;
                    isFirst = false;
                }
                card.CardSet(inst);
                _instanceMap.TryAdd(inst, card);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_transform);
        }

        public void Refresh(CodexInstance inst)
        {
            if (_instanceMap == null) return;
            if (_instanceMap.TryGetValue(inst, out var card))
            {
                card.CardSet(inst);
                if (card.IsReceived)
                {
                    Destroy(card.gameObject);
                    RebuildList(inst.CodexFaction, inst.CodexStd);
                }
            }
        }
    }
}