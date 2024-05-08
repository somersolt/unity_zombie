using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.AI;
using System.Collections.Generic;
using Photon.Realtime;
// 점수와 게임 오버 여부를 관리하는 게임 매니저
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // 싱글톤 접근용 프로퍼티
    public static GameManager instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<GameManager>();
            }

            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    public CinemachineVirtualCamera  virtualCamera;

    private static GameManager m_instance; // 싱글톤이 할당될 static 변수

    private int score = 0; // 현재 게임 점수
    public bool isGameover { get; private set; } // 게임 오버 상태

    private List<Player> diePlayer = new List<Player> ();

    [PunRPC]
    public void OnDie(Player player)
    {
        if (!diePlayer.Contains(player))
        {
            diePlayer.Add(player);
            if (diePlayer.Count >= PhotonNetwork.PlayerList.Length)
            {
                // 게임오버
                photonView.RPC("EndGame", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void OnRespawn(Player player)
    {
        diePlayer.Remove(player);
    }
    private void Awake() 
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        var pos = Random.insideUnitSphere * 10;
        if (NavMesh.SamplePosition(pos, out var hit, 10, NavMesh.AllAreas))
        { pos = hit.position; }
        else
        {
            pos = Vector3.zero;
        }    

        var woman = PhotonNetwork.Instantiate("Woman", pos , Quaternion.identity);
        virtualCamera.Follow = woman.transform;
        virtualCamera.LookAt = woman.transform;
        // 플레이어 캐릭터의 사망 이벤트 발생시 게임 오버
        //FindObjectOfType<PlayerHealth>().onDeath += EndGame;

        var playerHealth = woman.GetComponent<PlayerHealth>();
        playerHealth.onDeath += () => photonView.RPC("OnDie", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        playerHealth.OnRespawn += () => photonView.RPC("OnRespawn", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    // 점수를 추가하고 UI 갱신
    public void AddScore(int newScore) 
    {
        // 게임 오버가 아닌 상태에서만 점수 증가 가능
        if (!isGameover)
        {
            // 점수 추가
            score += newScore;
            // 점수 UI 텍스트 갱신
            UIManager.instance.UpdateScoreText(score);
        }
    }

    


    [PunRPC]
    // 게임 오버 처리
    public void EndGame() 
    {
        // 게임 오버 상태를 참으로 변경
        isGameover = true;
        // 게임 오버 UI를 활성화
        UIManager.instance.SetActiveGameoverUI(true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(score);
        }
        else
        {
            score = (int)stream.ReceiveNext();
            UIManager.instance.UpdateScoreText(score);
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}