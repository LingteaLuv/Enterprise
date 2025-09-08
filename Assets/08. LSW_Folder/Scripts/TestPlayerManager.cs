using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TestPlayerManager : MonoBehaviour
{
    public PlayerObj _prefabObj;
    public List<SPUM_Prefabs> _savedUnitList = new List<SPUM_Prefabs>();
    public Vector2 _startPos;
    public Vector2 _addPos;
    public int _columnNum;
    public int UnitMaxCount = 20;
    public Transform _playerPool;
    public List<PlayerObj> _playerList = new List<PlayerObj>();
    public PlayerObj _nowObj;
    public Transform _playerObjCircle;
    public Transform _goalObjCircle;
    public CommandPanel _commandPanel;
    
    private TestController _test;
    private InputAction _clickAction;
    
    private void Awake()
    {
        _test = new TestController();
        _test.Enable();
        _clickAction = _test.UI.Click;

        _clickAction.performed += ctx =>
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);
            
            if(hit.collider != null)
            {
                bool isHitPlayer = hit.collider.CompareTag("Player");
                _commandPanel.gameObject.SetActive(isHitPlayer);
               
                if(isHitPlayer)
                {
                    _nowObj = hit.collider.GetComponent<PlayerObj>();
                    _commandPanel.CreateAnimationPanel(_nowObj);
                }
                else
                {
                    //Set move Player object to this point
                    if(_nowObj!=null)
                    {
                        Vector2 goalPos = hit.point;
                        _goalObjCircle.transform.position = hit.point;
                        _nowObj.SetMovePos(goalPos);
                    }
                }
            }
        };
    }
    
    private void Start()
    {
        if(_savedUnitList.Count.Equals(0) || _playerList.Count.Equals(0))
            GetPlayerList();
    }
    // Update is called once per frame
    private void Update()
    {
        if(EventSystem.current.IsPointerOverGameObject()) return;

        if(_nowObj!=null)
        {
            _playerObjCircle.transform.position = _nowObj.transform.position;
        }
    }
    
    public void ClearPlayerList()
    {
        List<GameObject> tList = new List<GameObject>();
        for(var i =0; i < _playerPool.transform.childCount; i++)
        {
            GameObject tOBjj = _playerPool.transform.GetChild(i).gameObject;
            tList.Add(tOBjj);
        }
        foreach(var obj in tList)
        {
            DestroyImmediate(obj);
        }

        //net Edited. 2022.01.18
        _savedUnitList.Clear();
        _playerList.Clear();
    }
    
    public void GetPlayerList()
    {
        ClearPlayerList();

        var saveArray = Resources.LoadAll<SPUM_Prefabs>("");
        foreach (var unit in saveArray)
        {
            if (unit.ImageElement.Count > 0)
            {
                _savedUnitList.Add(unit);
                unit.PopulateAnimationLists();
            }
        }
        
        float numXStart = _startPos.x;
        float numYStart = _startPos.y;

        float numX = _addPos.x;
        float numY = _addPos.y;
        float ttV = 0;

        int sColumnNum = _columnNum;

        for(var i = 0 ; i < UnitMaxCount;i++)
        {
            if(i > _savedUnitList.Count-1) continue;
            if(i > sColumnNum-1)
            {
                numYStart -= 1f;
                numXStart -= numX * _columnNum;
                sColumnNum += _columnNum;
                ttV += numY;
            }
            
            GameObject ttObj = Instantiate(_prefabObj.gameObject, _playerPool);
            ttObj.transform.localScale = new Vector3(1,1,1);
            

            var tObj = Instantiate(_savedUnitList[i], ttObj.transform);
            tObj.transform.localScale = new Vector3(1,1,1);
            tObj.transform.localPosition = Vector3.zero;

            ttObj.name = _savedUnitList[i].name;

            PlayerObj tObjST = ttObj.GetComponent<PlayerObj>();

            tObjST._prefabs = tObj;

            ttObj.transform.localPosition = new Vector3(numXStart + numX * i,numYStart+ttV,0);
            _playerList.Add(tObjST);
        }
    }
    
    public void SetAlignUnits()
    {
        float numXStart = _startPos.x;
        float numYStart = _startPos.y;

        float numX = _addPos.x;
        float numY = _addPos.y;
        float ttV = 0;

        int sColumnNum = _columnNum;
        _playerList = _playerList.Where(s => s != null).ToList();
        for(var i = 0 ; i < _playerList.Count-1;i++)
        {
            if(i > sColumnNum-1)
            {
                numYStart -= 1f;
                numXStart -= numX * _columnNum;
                sColumnNum += _columnNum;
                ttV += numY;
            }
            
            GameObject ttObj = _playerList[i].gameObject;

            ttObj.transform.localPosition = new Vector3(numXStart + numX * i,numYStart+ttV,0);
        }
    }
}
