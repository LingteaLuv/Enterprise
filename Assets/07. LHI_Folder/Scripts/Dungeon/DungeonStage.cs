using System.Collections.Generic;
using UnityEngine;

namespace DungeonSystem
{
    /// <summary>
    /// 구역 타입 정의
    /// </summary>
    public enum ZoneType
    {
        NormalBattle,    // 일반 전투
        EliteBattle,     // 엘리트 전투
        BossBattle,      // 보스 전투
        RestRoom,        // 휴식방
        TreasureRoom     // 보물방
    }

    /// <summary>
    /// 구역 정보 클래스
    /// </summary>
    [System.Serializable]
    public class Zone
    {
        public int zoneId;
        public ZoneType zoneType;
        public bool isCompleted;
        public bool isUnlocked;

        public Zone(int id, ZoneType type)
        {
            zoneId = id;
            zoneType = type;
            isCompleted = false;
            isUnlocked = false;
        }
    }

    /// <summary>
    /// 스테이지(계층) 정보 클래스
    /// </summary>
    [System.Serializable]
    public class Stage
    {
        public int stageId;
        public List<Zone> zones;
        public bool isCompleted; // 스테이지 완료 여부
        public bool isUnlocked; // 스테이지 해금 여부
        public int currentZoneIndex; // 현재 진행 중인 구역 인덱스

        // 스테이지 생성자
        public Stage(int id, int zoneCount = 6)
        {
            stageId = id;
            zones = new List<Zone>();
            isCompleted = false;
            isUnlocked = false;
            currentZoneIndex = 0;

            GenerateZones(zoneCount);
        }

        /// <summary>
        /// 구역 생성 로직
        /// </summary>
        private void GenerateZones(int zoneCount)
        {
            // 구역 수가 3 이하이면 생성하지 않음
            if (zoneCount < 3) return;

            // 첫 구역은 항상 일반 전투 구역
            zones.Add(new Zone(0, ZoneType.NormalBattle));
            int TreasureRoomCount = 0;

            // 첫 구역과 마지막 두 구역을 제외한 나머지 구역 생성
            for (int i = 1; i < zoneCount - 2; i++)
            {
                // 랜덤 구역 타입 변수
                ZoneType randomType;

                // 보물방이 2개 미만일 경우, 포함하여 랜덤 선택
                if (TreasureRoomCount < 2)
                {
                    randomType = GetRandomZoneType();
                }
                else // 보물방이 2개 이상일 경우, 보물방 제외한 랜덤 선택
                {
                    randomType = GetRandomZoneTypeExcludingTreasureRoom();
                }

                // 보물방 개수 카운트
                if (randomType == ZoneType.TreasureRoom)
                {
                    TreasureRoomCount++;
                }

                // 구역 추가
                zones.Add(new Zone(i, randomType));
            }

            // 보스 구역 전은 항상 휴식방
            zones.Add(new Zone(zoneCount - 2, ZoneType.RestRoom));

            // 마지막 구역은 보스 구역으로 생성
            zones.Add(new Zone(zoneCount - 1, ZoneType.BossBattle));
        }

        /// <summary>
        /// 랜덤 구역 타입 결정, 추후 생성조건 추가하여 수정 필요
        /// </summary>
        private ZoneType GetRandomZoneType()
        {
            float rand = Random.Range(0f, 1f);

            if (rand < 0.5f) return ZoneType.NormalBattle;      // 50%
            else if (rand < 0.7f) return ZoneType.EliteBattle;  // 20%
            else if (rand < 0.85f) return ZoneType.RestRoom;    // 15%
            else return ZoneType.TreasureRoom;                  // 15%
        }

        /// <summary>
        /// 랜덤 구역 타입 결정 (보물방 제외)
        /// </summary>
        /// <returns></returns>
        private ZoneType GetRandomZoneTypeExcludingTreasureRoom()
        {
            float rand = Random.Range(0f, 1f);

            if (rand < 0.55f) return ZoneType.NormalBattle;      // 55%
            else if (rand < 0.8f) return ZoneType.EliteBattle;  // 25%
            else return ZoneType.RestRoom;                      // 20%
        }

        /// <summary>
        /// 현재 구역 완료 처리
        /// </summary>
        public void CompleteCurrentZone()
        {
            if (currentZoneIndex < zones.Count)
            {
                zones[currentZoneIndex].isCompleted = true;

                // 다음 구역 해금
                if (currentZoneIndex + 1 < zones.Count)
                {
                    zones[currentZoneIndex + 1].isUnlocked = true;
                    currentZoneIndex++;
                }
                else
                {
                    // 스테이지 완료
                    isCompleted = true;
                }
            }
        }

        public Zone GetCurrentZone()
        {
            if (currentZoneIndex < zones.Count)
                return zones[currentZoneIndex];
            return null;
        }
    }

    /// <summary>
    /// 던전 스테이지_ 던전의 스테이지와 구역 관리
    /// </summary>
    public class DungeonStage : MonoBehaviour
    {
        [Header("던전 설정")]
        [SerializeField] private int maxStages = 3;
        [SerializeField] private int minZonesPerStage = 5;
        [SerializeField] private int maxZonesPerStage = 7;

        [Header("현재 상태")]
        [SerializeField] private int currentStageIndex = 0; // 현재 스테이지 인덱스
        [SerializeField] private List<Stage> stages;

        // 이벤트
        public System.Action<Stage> OnStageChanged;
        public System.Action<Zone> OnZoneChanged;
        public System.Action<ZoneType> OnZoneCompleted;

        void Start()
        {
            InitializeDungeon();
        }

        // 던전 초기화
        public void InitializeDungeon()
        {
            stages = new List<Stage>();

            // 스테이지 생성
            for (int i = 0; i < maxStages; i++)
            {
                int zoneCount = Random.Range(minZonesPerStage, maxZonesPerStage + 1);
                Stage newStage = new Stage(i, zoneCount);

                stages.Add(newStage);
            }
            // 첫 번째 스테이지, 구역 만 해금
            stages[0].isUnlocked = true;
            stages[0].zones[0].isUnlocked = true;

            Debug.Log($"던전 초기화 완료: {stages.Count}개 스테이지 생성");
            OnStageChanged?.Invoke(GetCurrentStage());
        }

        // 현재 스테이지 반환
        public Stage GetCurrentStage()
        {
            if (currentStageIndex < stages.Count)
                return stages[currentStageIndex];
            return null;
        }

        // 현재 구역 반환
        public Zone GetCurrentZone()
        {
            Stage currentStage = GetCurrentStage();
            return currentStage?.GetCurrentZone();
        }

        /// <summary>
        /// 구역 완료 처리
        /// </summary>
        [ContextMenu("구역 완료 처리")]
        public void CompleteCurrentZone()
        {
            Stage currentStage = GetCurrentStage();
            Zone currentZone = GetCurrentZone();

            if (currentStage != null && currentZone != null)
            {
                currentStage.CompleteCurrentZone(); // 구역 완료 처리
                OnZoneCompleted?.Invoke(currentZone.zoneType);

                // 스테이지 완료 체크
                if (currentStage.isCompleted)
                {
                    Debug.Log($"스테이지 {currentStage.stageId} 완료!");
                    MoveToNextStage();
                }
                else
                {
                    OnZoneChanged?.Invoke(GetCurrentZone());
                }
            }
        }

        /// <summary>
        /// 다음 스테이지로 이동 (스테이지 마지막, 보스 클리어 이후 실행)
        /// </summary>
        public void MoveToNextStage()
        {
            if (currentStageIndex + 1 < stages.Count)
            {
                currentStageIndex++;
                stages[currentStageIndex].isUnlocked = true;
                stages[currentStageIndex].zones[0].isUnlocked = true;

                Debug.Log($"스테이지 {currentStageIndex}로 이동");
                OnStageChanged?.Invoke(GetCurrentStage());
                OnZoneChanged?.Invoke(GetCurrentZone());
            }
            else
            {
                Debug.Log("던전 완료!");
            }
        }

        // 디버그용 정보 출력
        [ContextMenu("현재 상태 출력")]
        public void PrintCurrentStatus()
        {
            Stage stage = GetCurrentStage();
            Zone zone = GetCurrentZone();

            if (stage != null && zone != null)
            {
                Debug.Log($"현재 위치: 스테이지 {stage.stageId + 1}, 구역 {stage.currentZoneIndex + 1}/{stage.zones.Count}");
                Debug.Log($"구역 타입: {zone.zoneType}");
                Debug.Log($"스테이지 진행도: {GetStageProgress(stage):P0}");

                // 모든 스테이지 및 구역 출력
                for (int i = 0; i < stages.Count; i++)
                {
                    Stage s = stages[i];
                    string stageInfo = $"스테이지 {s.stageId + 1} - 완료: {s.isCompleted}, 해금: {s.isUnlocked}, 구역 수: {s.zones.Count}";
                    for (int j = 0; j < s.zones.Count; j++)
                    {
                        Zone z = s.zones[j];
                        stageInfo += $"\n  구역 {z.zoneId + 1} - 타입: {z.zoneType}, 완료: {z.isCompleted}, 해금: {z.isUnlocked}";
                    }
                    Debug.Log(stageInfo);
                }
            }
        }

        // 스테이지 진행도 계산
        public float GetStageProgress(Stage stage)
        {
            if (stage == null || stage.zones.Count == 0) return 0f;

            int completedZones = 0;
            foreach (Zone zone in stage.zones)
            {
                if (zone.isCompleted) completedZones++;
            }

            return (float)completedZones / stage.zones.Count;
        }

        // 전체 던전 진행도 계산
        public float GetDungeonProgress()
        {
            if (stages.Count == 0) return 0f;

            int completedStages = 0;
            foreach (Stage stage in stages)
            {
                if (stage.isCompleted) completedStages++;
            }

            return (float)completedStages / stages.Count;
        }

        // 특정 스테이지의 구역 정보 반환
        public List<Zone> GetStageZones(int stageIndex)
        {
            if (stageIndex >= 0 && stageIndex < stages.Count)
                return stages[stageIndex].zones;
            return null;
        }
    }
}