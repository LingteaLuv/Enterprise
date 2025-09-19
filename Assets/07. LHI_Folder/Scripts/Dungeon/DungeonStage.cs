using System.Collections.Generic;
using UnityEngine;

namespace DungeonSystem
{ 
    /// <summary>
    /// 구역 타입 정의
    /// </summary>
    public enum ZoneType
    {
        NormalBattle,   // 일반 전투
        EliteBattle,    // 엘리트 전투
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

            // 보물방과 휴식방 카운트
            int treasureRoomCount = 0;
            int restRoomCount = 0;
            int maxTreasureRooms = (stageId == 1 || stageId == 2) ? 1 : (stageId == 3 ? 2 : 0);
            int maxRestRooms = 1;

            // 첫 구역과 마지막 두 구역을 제외한 나머지 구역 생성
            for (int i = 1; i < zoneCount - 2; i++)
            {
                ZoneType randomType = RandomZoneBattle(); // 기본값으로 전투 구역 설정
                bool roomDecided = false;
                float rand = Random.Range(0f, 1f);

                // 보물방과 휴식방을 우선적으로 배치
                if (treasureRoomCount < maxTreasureRooms)
                {
                    if (rand < 0.33f) // 예시 확률: 25% 확률로 보물방
                    {
                        randomType = ZoneType.TreasureRoom;
                        treasureRoomCount++;
                        roomDecided = true;
                    }
                }

                if (!roomDecided && restRoomCount < maxRestRooms)
                {
                    if (rand < 0.66f) // 예시 확률: 25% 확률로 휴식방
                    {
                        randomType = ZoneType.RestRoom;
                        restRoomCount++;
                        roomDecided = true;
                    }
                }

                // 보물방과 휴식방이 결정되지 않았다면 전투 구역으로 설정
                if (!roomDecided)
                {
                    randomType = RandomZoneBattle();
                }

                zones.Add(new Zone(i, randomType));
            }

            // 보스 구역 전은 항상 휴식방
            zones.Add(new Zone(zoneCount - 2, ZoneType.RestRoom));

            // 마지막 구역은 보스 구역으로 생성
            zones.Add(new Zone(zoneCount - 1, ZoneType.BossBattle));
        }

        /// <summary>
        /// 전투방 랜덤 설정
        /// </summary>
        private ZoneType RandomZoneBattle()
        {
            // 전투방 랜덤 설정
            float ran = Random.Range(0f, 1f);
            if (ran < 0.6f) return ZoneType.NormalBattle;      // 60% 일반 전투
            else return ZoneType.EliteBattle;                 // 40% 엘리트 전투
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
    public class DungeonStage : Singleton<DungeonStage>
    {
        [Header("던전 설정")]
        [SerializeField] private int maxStages = 3;
        [SerializeField] private int maxZones = 7;

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
                Stage newStage = new Stage(i, maxZones);

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

        // 구역 완료 메서드
        public void CompleteCurrentZone()
        {
            Stage currentStage = GetCurrentStage();
            if (currentStage != null)
            {
                Zone currentZone = currentStage.GetCurrentZone();
                if (currentZone != null && !currentZone.isCompleted)
                {
                    currentStage.CompleteCurrentZone();
                    Debug.Log($"스테이지 {currentStage.stageId}의 구역 {currentZone.zoneId} 완료: {currentZone.zoneType}");
                    OnZoneCompleted?.Invoke(currentZone.zoneType);
                    // 구역 변경 이벤트 호출
                    OnZoneChanged?.Invoke(currentStage.GetCurrentZone());
                    // 스테이지 완료 시 다음 스테이지로 이동
                    if (currentStage.isCompleted)
                    {
                        Debug.Log($"스테이지 {currentStage.stageId} 완료!");
                        MoveToNextStage();
                    }
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