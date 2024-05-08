using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SocialPlatforms.Impl;
using System.IO;
using System.Collections;
using ExitGames.Client.Photon;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviourPun, IPunObservable
{
    public Enemy enemyPrefab; // 생성할 적 AI

    public ZombieData[] zombieDatas;
    public Transform[] spawnPoints; // 적 AI를 소환할 위치들

    public float damageMax = 40f; // 최대 공격력
    public float damageMin = 20f; // 최소 공격력

    public float healthMax = 200f; // 최대 체력
    public float healthMin = 100f; // 최소 체력

    public float speedMax = 3f; // 최대 속도
    public float speedMin = 1f; // 최소 속도

    public Color strongEnemyColor = Color.red; // 강한 적 AI가 가지게 될 피부색

    private List<Enemy> enemies = new List<Enemy>(); // 생성된 적들을 담는 리스트

    private int enemieCount;
    private int wave; // 현재 웨이브

    public ItemSpawner itemSpawner;

    private void Awake()
    {
        PhotonPeer.RegisterType(typeof(Color), 128, ColorSerialization.SerializeColor, ColorSerialization.DeserializeColor);
    }

    private void Update() {

        if (PhotonNetwork.IsMasterClient)
        {
            // 게임 오버 상태일때는 생성하지 않음
            if (GameManager.instance != null && GameManager.instance.isGameover)
            {
                return;
            }

            // 적을 모두 물리친 경우 다음 스폰 실행
            if (enemies.Count <= 0)
            {
                wave++;
                SpawnWave();
            }
        }


        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 표시
    private void UpdateUI() 
    {
        // 현재 웨이브와 남은 적의 수 표시
        UIManager.instance.UpdateWaveText(wave, enemieCount);
    }

    // 현재 웨이브에 맞춰 적을 생성
    private void SpawnWave()
    {
        float total = 0;
        enemies.Clear();
        for (int j = 0; j < zombieDatas.Length; j++)
        {
            total += zombieDatas[j].percentage;
        }
        for (int i = 0; i < 5; ++i)
        {
            float a = Random.Range(0, total);
            float b = 0;
            int count = 0;
            while (a > b)
            {
                b += zombieDatas[count].percentage;
                count++;
            }
            CreateEnemy(zombieDatas[count - 1]);
        }

    }


    private void CreateEnemy(ZombieData data)
    {
        var go = PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity );
        var enemy = go.GetComponent<Enemy>();
        enemy.photonView.RPC("Setup", RpcTarget.AllBuffered,
            data.health,
            data.damage,
            data.speed,
            data.skinColor
            );
        
        //델리게이트 이벤트 (죽을때 발생할 일들 연결)
        enemy.onDeath += () =>
        {
            itemSpawner.SpawnFromEnemy(enemy.transform.position);
            enemies.Remove(enemy);
            StartCoroutine(CoDestroyAfter(go, 5f));
            enemieCount = enemies.Count;
            GameManager.instance.AddScore(Mathf.FloorToInt(100));
        };

        enemies.Add(enemy);
        enemieCount = enemies.Count;

    }

    //// 적을 생성하고 생성한 적에게 추적할 대상을 할당
    //private void CreateEnemy(float intensity) 
    //{
    //    var go = PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoints[Random.Range(0, spawnPoints.Length)].position, spawnPoints[Random.Range(0, spawnPoints.Length)].rotation);

    //    var enemy = go.GetComponent<Enemy>();

    //    enemy.photonView.RPC("Setup", RpcTarget.All,
    //        Mathf.Lerp(healthMin, healthMax, intensity),
    //        Mathf.Lerp(damageMin, damageMax, intensity),
    //        Mathf.Lerp(speedMin, speedMax, intensity),
    //        Color.Lerp(Color.white, strongEnemyColor, intensity / 10)
    //        );


    //    //델리게이트 이벤트 (죽을때 발생할 일들 연결)
    //    enemy.onDeath += () =>
    //    {
    //        //itemSpawner.SpawnFromEnemy(enemy.transform.position);
    //        StartCoroutine(CoDestroyAfter(go, 5f));
    //        enemies.Remove(enemy);
    //        enemieCount = enemies.Count;
    //        GameManager.instance.AddScore(Mathf.FloorToInt(10 * intensity));
    //    };
    //    enemies.Add(enemy);
    //    enemieCount = enemies.Count;
    //    var live = enemy.GetComponent<LivingEntity>();

    //}

    IEnumerator CoDestroyAfter(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        PhotonNetwork.Destroy(go);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(wave);
            stream.SendNext(enemieCount);
        }
        else
        {
            wave = (int)stream.ReceiveNext();
            enemieCount = (int)stream.ReceiveNext();
        }
        UpdateUI();
    }
}