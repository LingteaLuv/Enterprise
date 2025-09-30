using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _05._CSJ_Folder.Scripts.Quest;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    public static class CodexDataManager
    {
                
        public static bool _saveQueued;
        private static bool _isSaving;
        
        private static IEnumerable<CodexData> _data;
        private static string _dataPath;
        
        public static IEnumerator SaveDebounce(string dataPath, IEnumerable<CodexData> data ,float delaySec)
        {
            var codexDatas = data.ToList();
            var path = dataPath;
            
            _data = codexDatas;
            _dataPath = path;
            
            yield return new WaitForSeconds(delaySec);
            if (!_saveQueued) yield break;
            
            _saveQueued = false;
            yield return SaveAsync(dataPath, codexDatas).AsIEnumerator();
        }

        public static void TrySave()
        {
            try
            {
                if(_data is not null && !string.IsNullOrEmpty(_dataPath))
                    SaveAsync(_dataPath, _data).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.LogError($"즉시 저장 실패 {e}");
            }
        }
        
        private static async Task SaveAsync(string dataPath, IEnumerable<CodexData> data)
        {
            if (_isSaving) return;
            _isSaving = true;
            try
            {
                // 데이터베이스 매니저에 해당 데이터와 경로로 저장 대기
                await DatabaseManager.Instance.SaveCodexDataAsync(dataPath, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"퀘스트 저장 실패 {e}");
            }
            finally
            {
                _isSaving = false;
            }

        }
    }
}