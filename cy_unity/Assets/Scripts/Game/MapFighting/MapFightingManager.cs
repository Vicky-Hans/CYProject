using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DH.Game
{
    public class MapFightingManager : FightingBaseManager
    {
        public override int FightType => (int)EStateType.StageTypeSecret;
        private Dictionary<int, Sprite> bgDictionary = new();
        private Dictionary<int,Sprite> edgeDictionary = new();

        private const float BgCellSize = 5.12f;
        private const float EdgeCellSize = 2.56f;
        private float waveCheckInterval = 0f;
        private float endCheckInterval = 0f;

        private GameObject areaSpriteObj; // boss area edge
        private SpriteRenderer areaSpriteRenderer;

        public Vector2 MapRange { get; set; }
        public float ClearMonsterDistance { get; set; }  // 清除怪物的距离
        public float MaxMonsterCount { get; set; }  // 最多怪物数量
        public PhysicsMaterial2D monsterPhyMat;
        public GameObject bossWaringFx;
        private Collider2D PlayerCollider { get; set; }
        public MonsterController CurrentBoss { get; set; }
        public Vector3 BossBornPos { get; set; }
        public Vector2 BossAreaSize { get; set; }
        public List<GameObject> bossEdgeGameObjectList = new List<GameObject>();
        private ForestSpawnManager ForestSpawnManager => spawnManager as ForestSpawnManager;
        private CopySecretCfg copySecretCfg;
        protected override void Start()
        {
            base.Start();
            transform.position = Vector3.zero;
            // 初始化战斗管理器
            BattleManager.Instance.fightingManagerIns = this;
            PlayerCollider = playerCtrl.GetComponent<Collider2D>();
            PlayerCollider.isTrigger = true;
            var playerRgb2d = playerCtrl.GetComponent<Rigidbody2D>();
            playerRgb2d.freezeRotation = true;
            playerRgb2d.mass = 1000f;
            copySecretCfg = ConfigCenter.CopySecretCfgColl.GetDataById(GameManager.Instance.CurChapterId);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"), false);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Ground"), false);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), false);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
        }

        public async UniTask Init()
        {
            entityPool.forceDisableChildrenManage = true;
            await BaseInit();
            //playerCtrl.transform.position = Vector3.zero;
            CalPlayerPos();
            //MapRange = new Vector2(40f, 40f);
            CalMapRange();
            enemyManager ??= new EnemyManager();
            dropManager ??= new DropManager();
            dropManager.DropList.Clear();
            dropManager.Init();
            spawnManager ??= new ForestSpawnManager();
            gTime = 0f;
            inited = true;
            FightingSoundHelper.Instance.PlayBgm();
            await InitBg(1);
            await LoadAreaSpriteObj("Player/ForestAreaEdge");
            if (camCtrl != null)
            {
                camCtrl.SetTarget(playerCtrl.transform);
                camCtrl.MapRange = MapRange;
                camCtrl.ScreenSize = ScreenSize;
            }
            GetDefines();
            // test start
            // StartWave(1, 1);
        }

        private void CalMapRange()
        {
            var cfg = ConfigCenter.CopySecretCfgColl.GetDataById(GameDataManager.Instance.CurChapterId);
            if (cfg == null || cfg.MapSize == null || cfg.MapSize.Count < 2)
            {
                var tempCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_04);
                MapRange = new Vector2(tempCfg.Content[0], tempCfg.Content[1]) * GameConst.AttributeDivisor;
            }
            else
            {
                MapRange = new Vector2(cfg.MapSize[0], cfg.MapSize[1]) * GameConst.AttributeDivisor;
            }
        }

        private void CalPlayerPos()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_02);
            var pos = new Vector3(cfg.Content[0], cfg.Content[1], 0) * GameConst.AttributeDivisor;
            playerCtrl.transform.localPosition = pos;
        }

        private void GetDefines()
        {
            var defineCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_18);
            ClearMonsterDistance = defineCfg.Content[0] * GameConst.AttributeDivisor;
            defineCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_19);
            MaxMonsterCount = defineCfg.Content[0];
        }

        private async UniTask LoadAreaSpriteObj(string path)
        {
            areaSpriteObj = await AssetsManager.LoadAssetAsync<GameObject>(path);
            areaSpriteRenderer = areaSpriteObj.GetComponent<SpriteRenderer>();
        }

        private async UniTask LoadBg(int index, string path)
        {
            var tempSprite = await AssetsManager.LoadAssetAsync<Sprite>(path);
            bgDictionary.Add(index - 1,tempSprite);
        }
        private async UniTask LoadEdge(int index, string path)
        {
            var tempSprite = await AssetsManager.LoadAssetAsync<Sprite>(path);
            edgeDictionary.Add(index - 9,tempSprite);
        }

        private async UniTask LoadBgAndEdge()
        {
            var bgPath = "Fighting/Bgs/forest_{0}_{1}";
            var cfg = ConfigCenter.CopySecretCfgColl.GetDataById(GameDataManager.Instance.CurChapterId);
            var mapIdStr = "1";
            if (cfg != null && cfg.BattleMap != null)
            {
                mapIdStr = cfg.BattleMap;
            }

            bgDictionary.Clear();
            var taskArray = new UniTask[12];
            for (int i = 1; i <= 12; i++)
            {
                var tempPath = string.Format(bgPath, mapIdStr,i);
                if (i <= 8)
                {
                    taskArray[i - 1] = LoadBg(i, tempPath); 
                }
                else
                {
                    taskArray[i - 1] = LoadEdge(i, tempPath);
                }
            }
            await UniTask.WhenAll(taskArray);
        }

        protected override async UniTask InitBg(int stage)
        {
            // 加载资源
            await LoadBgAndEdge();
            
            var fightBgTrans = fightBg.transform;
            var parentTrans = fightBgTrans.parent;
            parentTrans.localPosition = Vector3.zero;
            var bgObjList = new List<GameObject>();
            var hCount = Mathf.CeilToInt(MapRange.x / BgCellSize);
            var vCount = Mathf.CeilToInt(MapRange.y / BgCellSize);
            var xPos = -(hCount - 1) / 2f * BgCellSize;
            var yPos = -(vCount - 1) / 2f * BgCellSize;
            for (int i = 0; i < vCount; i++)
            {
                for(int j = 0; j < hCount; j++)
                {
                    var bgIdx = Random.Range(0, bgDictionary.Count);
                    var spriteObj = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
                    spriteObj.sprite = bgDictionary[bgIdx];
                    spriteObj.transform.localPosition = new Vector3(xPos + j * BgCellSize, yPos + i * BgCellSize);
                    bgObjList.Add(spriteObj.gameObject);
                }
            }
            bgObjList.ForEach(go => go.SetActive(true));
            var edgeObjList = new List<GameObject>();
            // horizontal edge
            hCount = Mathf.CeilToInt(MapRange.x / EdgeCellSize);
            vCount = Mathf.CeilToInt(MapRange.y / EdgeCellSize);
            xPos = -(hCount - 1) / 2f * EdgeCellSize;
            yPos = -(vCount - 1) / 2f * EdgeCellSize;
            for (int i = 0; i < hCount; i++)
            {
                var spriteObj = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition = new Vector3(xPos + i * EdgeCellSize, MapRange.y/2f-EdgeCellSize/2f);
                spriteObj.sprite = edgeDictionary[Random.Range(0, edgeDictionary.Count)];
                edgeObjList.Add(spriteObj.gameObject);
                spriteObj = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition = new Vector3(xPos + i * EdgeCellSize, -(MapRange.y/2-EdgeCellSize/2f));
                spriteObj.sprite = edgeDictionary[Random.Range(0, edgeDictionary.Count)];
                edgeObjList.Add(spriteObj.gameObject);
            }
            // vertical edge
            for (int i = 0; i < vCount*2; i++)
            {
                var spriteObj =
                    Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition =
                    new Vector3(-MapRange.x / 2f + EdgeCellSize / 2f, yPos + i * EdgeCellSize/2f);
                spriteObj.sprite = edgeDictionary[Random.Range(0, edgeDictionary.Count)];
                edgeObjList.Add(spriteObj.gameObject);
                spriteObj = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition = new Vector3(MapRange.x / 2 - EdgeCellSize / 2f,
                    yPos + i * EdgeCellSize/2f);
                spriteObj.sprite = edgeDictionary[Random.Range(0, edgeDictionary.Count)];
                edgeObjList.Add(spriteObj.gameObject);
            }
            edgeObjList.ForEach(go =>
            {
                go.GetComponent<SpriteRenderer>().sortingOrder = 1;
                go.SetActive(true);
            });
        }
        
        public override void StartWave(int stageId, int waveId)
        {
            stage = stageId;
            wave = waveId;
            GameTime.Instance.Pause = false;
            spawnManager.Init(stageId, waveId);
            spawnManager.PreGenerateMonster();
            spawnManager.StartSpawn();
            playerCtrl.WeaponSkillController.CheckedWaveStart = false;
        }

        private Vector3 GetWorldPosRelativePlayer(Vector3 pos)
        {
            if (playerCtrl != null) return playerCtrl.transform.position + pos;
            return Vector3.zero;
        }
        
        public MonsterController CreateMonster(int monsterId, int entry, int count = 1, int idx = 0,
            int modelId = 0, bool isSplitBody = false, SecretGrinDingCfg grindingCfg = null)
        {
            var cfg = ConfigCenter.MonsterCfgColl.GetDataById(monsterId);
            var modelName = modelId>0? $"monster_{modelId}" : GetModelName(cfg.ModelId);
            var assetPath = $"Fighting/Enemy/{modelName}";
            entityPool.LoadAssetSync(assetPath);
            var pos = GetEntryPos(entry, count, idx);
            var go = entityPool.InstantiateObj(assetPath, GetWorldPosRelativePlayer(pos), Quaternion.identity, transform);
            if (cfg.MonsterType == (int)MonsterType.Boss)
            {
                go.transform.position = BossBornPos;
            }
            var obj = go.GetComponent<MonsterController>();
            obj.IsSplitBody = isSplitBody;
            // obj.TargetTrans = playerCtrl.transform;
            var targetPos = GetTargetPos(pos, entry, obj);
            obj.Init(cfg, GetWorldPosRelativePlayer(targetPos), null, entityPool);
            if(!isSplitBody && grindingCfg != null)
            {
               
            }
            
            var entryCfg = ConfigCenter.MonsterEnterCfgColl.GetDataById(entry);
            if (entryCfg.MoveType > 0)
            {
                obj.MoveType = (EMonsterMoveType)entryCfg.MoveType;
            }

            if (cfg.MonsterType == (int)MonsterType.Boss && !isSplitBody)
            {
                BossCount++;
                CurrentBoss = obj;
            }

            obj.MonsterEntry = entry;
            enemyManager.Add(obj);
            return obj;
        }

        private void PlayBossWaring()
        {
            var fxObj = Instantiate(bossWaringFx, BossBornPos, Quaternion.identity, fightPanelTrans);
            if(fxObj != null)
            {
                AddAutoReleaseGObj(fxObj, 2f);
            }
        }

        public override void ShowBossComing()
        {
            ClearAllMonster(false);
            base.ShowBossComing();
            GameDataManager.Instance.ShowBossInfo = true;
            ShowBossArea();
            PlayBossWaring();
            PlayerCollider.isTrigger = false;
        }

        public void ShowBossArea()
        {
            var fightBgTrans = fightBg.transform;
            var parentTrans = fightBgTrans.parent;
            var areaCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_26);
            var areaSize = new Vector2(areaCfg.Content[0], areaCfg.Content[1]) * GameConst.AttributeDivisor;
            BossAreaSize = areaSize;
            var centerPos = playerCtrl.transform.position;
            var centerLocalPos = parentTrans.InverseTransformPoint(centerPos);
            const float areaOffset = 0f;
            centerLocalPos.x = Mathf.Clamp(centerLocalPos.x, -MapRange.x / 2 + areaSize.x / 2 + areaOffset, MapRange.x / 2 - areaSize.x / 2 - areaOffset);
            centerLocalPos.y = Mathf.Clamp(centerLocalPos.y, -MapRange.y / 2 + areaSize.y / 2 + areaOffset, MapRange.y / 2 - areaSize.y / 2 - areaOffset);
            BossBornPos = parentTrans.TransformPoint(centerLocalPos);
            
            var edgeSprSize = areaSpriteRenderer.size;
            
            var edgeObjList = new List<GameObject>();
            // horizontal edge
            var hCount = Mathf.CeilToInt(areaSize.x / edgeSprSize.x);
            var vCount = Mathf.CeilToInt(areaSize.y / edgeSprSize.y);
            var xPos = -(hCount - 1) / 2f * edgeSprSize.x + centerLocalPos.x;
            var yPos = -(vCount - 1) / 2f * edgeSprSize.y + centerLocalPos.y;
            for (int i = 0; i < hCount+1; i++)
            {
                var spriteObj = Instantiate(areaSpriteRenderer, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition = new Vector3(xPos + i * edgeSprSize.x, yPos+areaSize.y);
                spriteObj.sprite = areaSpriteRenderer.sprite;
                edgeObjList.Add(spriteObj.gameObject);
                spriteObj = Instantiate(areaSpriteRenderer, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition = new Vector3(xPos + i * edgeSprSize.x, yPos);
                spriteObj.sprite = areaSpriteRenderer.sprite;
                edgeObjList.Add(spriteObj.gameObject);
            }
            // vertical edge
            for (int i = 0; i < vCount*2 + 1; i++)
            {
                var spriteObj =
                    Instantiate(areaSpriteRenderer, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition =
                    new Vector3(xPos, yPos + i * edgeSprSize.y/2f);
                spriteObj.sprite = areaSpriteRenderer.sprite;
                edgeObjList.Add(spriteObj.gameObject);
                spriteObj = Instantiate(areaSpriteRenderer, Vector3.zero, Quaternion.identity, parentTrans);
                spriteObj.transform.localPosition = new Vector3(xPos + areaSize.x,
                    yPos + i * edgeSprSize.y/2f);
                spriteObj.sprite = areaSpriteRenderer.sprite;
                edgeObjList.Add(spriteObj.gameObject);
            }
            edgeObjList.ForEach(go =>
            {
                bossEdgeGameObjectList.Add(go);
                AddStaticEdge(go, edgeSprSize);
                go.GetComponent<SpriteRenderer>().sortingOrder = 2;
                go.SetActive(true);
            });
        }

        private void AddStaticEdge(GameObject obj, Vector2 oSize)
        {
            obj.layer = LayerMask.NameToLayer("Ground");
            var boxCollider2D = obj.AddComponent<BoxCollider2D>();
            boxCollider2D.size = oSize;
            var rgb2D = obj.AddComponent<Rigidbody2D>();
            rgb2D.isKinematic = false;
            rgb2D.bodyType = RigidbodyType2D.Static;
            boxCollider2D.isTrigger = false;
            rgb2D.gravityScale = 0f;
            rgb2D.freezeRotation = true;
        }

        private void ClearBossEdgeObjects()
        {
            bossEdgeGameObjectList.ForEach(Destroy);
            bossEdgeGameObjectList.Clear();
            PlayerCollider.isTrigger = true;
        }

        private void Update()
        {
            if(!inited) return;
            if(GameTime.Instance.Pause) return;
            var dt = Time.deltaTime;
            var unscaledDt = Time.unscaledDeltaTime;
            gTime += dt;
            GameTime.Instance.OnUpdate(dt);
            TimerManager.Instance.Update(dt);
            enemyManager.Update(dt);
            dropManager.Update(dt);
            playerCtrl.OnUpdate(dt);
            gamePlayManager.Update(dt, unscaledDt);
            lifeTimeManager.Update(dt, unscaledDt);
            CheckEnd(dt);
            CheckWaveEnd(dt);
        }

        public void OnWaveEnd()
        {
            if(BossCount > 0)return;
            ClearBossEdgeObjects();
            wave++;
            GameDataManager.Instance.WaveEnd = true;
            spawnManager.Init(1, wave);
            spawnManager.PreGenerateMonster();
            // 同步波次结束
            if (GameDataManager.Instance.Wave < copySecretCfg.MaxWave)
            {
                GameManager.Instance.RequestSecretBattleWaveDone().Forget();
            }
            spawnManager.StartSpawn();
            // 更新波次
            GameDataManager.Instance.Wave++;
            // 这里不停
            GameDataManager.Instance.WaveEnd = false;
        }
        
        private void CheckWaveEnd(float dt)
        {
            waveCheckInterval += dt;
            if (waveCheckInterval < 0.5f) return;
            waveCheckInterval = 0f;
            if(BossCount > 0)return;
            if(spawnManager.TotalSpawnCount <= 0 || spawnManager.CurrenSpawnCount < spawnManager.TotalSpawnCount)return;
            OnWaveEnd();
        }
        
        protected void CheckEnd(float dt)
        {
            endCheckInterval += dt;
            if (endCheckInterval < 0.5f) return;
            endCheckInterval = 0f;
            if(!ForestSpawnManager.AllWaveEnd)return;
            if (CheckAllMonsterIsDead())
            {
                playerCtrl.Player.resource.Armor = 0;
                //TO-DO: 战斗结束调用
                gamePlayManager.ClearAllBullets();
                GameDataManager.Instance.CurGameState = EGameState.Success;
            }
        }
        protected override void OnDestroy()
        {
            TimerManager.Instance.RemoveTimerByTag(GameConst.TimerTagName.Grinding);
            TimerManager.Instance.RemoveTimerByTag(GameConst.TimerTagName.PauseTask);
            PauseTask.RemoveAllItem();
            lifeTimeManager.Shutdown();
            lifeTimeManager = null;
            entityPool.ReleaseAssets();
            base.OnDestroy();
        }
    }
}