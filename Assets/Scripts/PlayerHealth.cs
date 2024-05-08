using Photon.Pun;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI; // UI 관련 코드

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerHealth : LivingEntity {

    public Slider healthSlider; // 체력을 표시할 UI 슬라이더

    public AudioClip deathClip; // 사망 소리
    public AudioClip hitClip; // 피격 소리
    public AudioClip itemPickupClip; // 아이템 습득 소리

    private AudioSource playerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    public event System.Action OnRespawn;

    private void Awake() 
    {
        // 사용할 컴포넌트를 가져오기
        playerAnimator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
        playerAudioPlayer = GetComponent<AudioSource>();

    }

    protected override void OnEnable() 
    {
        base.OnEnable();

        healthSlider.gameObject.SetActive(true);

        //hp정규화 안하고 그대로 쓰고 싶으면
        //healthSlider.minValue = 0f;
        //healthSlider.maxValue = startingHealth;

        healthSlider.value = health / startingHealth;
        playerMovement.enabled = true;
        playerShooter.enabled = true;

        UIManager.instance.SetActiveGameoverUI(false);
    }

    [PunRPC]
    // 체력 회복
    public override void RestoreHealth(float newHealth) 
    {
        // LivingEntity의 RestoreHealth() 실행 (체력 증가)
        base.RestoreHealth(newHealth);
        healthSlider.value = health / startingHealth;
    }

    [PunRPC]
    // 데미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitDirection) 
    {
        if(dead)
        {
            return;
        }
        base.OnDamage(damage, hitPoint, hitDirection);
        healthSlider.value = health / startingHealth;
        playerAudioPlayer.PlayOneShot(hitClip);
    }

    // 사망 처리
    public override void Die() 
    {
        base.Die();
        healthSlider.gameObject.SetActive(false);
        playerAudioPlayer.PlayOneShot(deathClip);
        playerAnimator.SetTrigger("Die");

        playerMovement.enabled = false;
        playerShooter.enabled = false;

        Invoke("Respawn", 5f);
        //UIManager.instance.SetActiveGameoverUI(true);

    }

    private void OnTriggerEnter(Collider other) 
    {
        if (dead || !PhotonNetwork.IsMasterClient)
            return;

        var item = other.GetComponent<IItem>();
        if (item != null)
        {
            item.Use(gameObject);
            playerAudioPlayer.PlayOneShot(itemPickupClip);
        }

    }
    public void Respawn()
    {
        if (OnRespawn != null)
        {
            OnRespawn();
        }

            gameObject.SetActive(false);
            gameObject.SetActive(true);

    }
}

