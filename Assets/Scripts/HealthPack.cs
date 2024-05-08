using Photon.Pun;
using Photon.Pun.Demo.SlotRacer.Utils;
using UnityEngine;

// 체력을 회복하는 아이템
public class HealthPack : MonoBehaviourPun, IItem {
    public float health = 50; // 체력을 회복할 수치

    [PunRPC]
    public void Use(GameObject target) {

        PlayerShooter playerShooter = target.GetComponent<PlayerShooter>();
        // 전달받은 게임 오브젝트로부터 LivingEntity 컴포넌트 가져오기 시도
        LivingEntity life = playerShooter.GetComponent<LivingEntity>();

        // LivingEntity컴포넌트가 있다면
        if (life != null)
        {
            // 체력 회복 실행
            life.photonView.RPC("RestoreHealth", RpcTarget.All, health);

        }

        // 사용되었으므로, 자신을 파괴
        PhotonNetwork.Destroy(gameObject);

    }
}