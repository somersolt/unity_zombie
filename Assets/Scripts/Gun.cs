using System.Collections;
using UnityEngine;
using Photon.Pun;

// 총을 구현한다
public class Gun : MonoBehaviourPun, IPunObservable
{
    // 총의 상태를 표현하는데 사용할 타입을 선언한다
    public enum State 
    {
        Ready, // 발사 준비됨
        Empty, // 탄창이 빔
        Reloading // 재장전 중
    }

    public State state { get; private set; } // 현재 총의 상태

    public Transform fireTransform; // 총알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 총알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기

    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄약
    public int magAmmo; // 현재 탄창에 남아있는 탄약

    private float lastFireTime; // 총을 마지막으로 발사한 시점

    public GunData gunData;

    private void Awake() 
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        bulletLineRenderer = GetComponent<LineRenderer>();
        gunAudioPlayer = GetComponent<AudioSource>();

        bulletLineRenderer.enabled = false;
        bulletLineRenderer.positionCount = 2;
    }

    private void OnEnable() 
    {
        magAmmo = gunData.magCapacity;
        ammoRemain = gunData.startAmmo;

        lastFireTime = 0;
        state = State.Ready;

        UIManager.instance.UpdateAmmoText(magAmmo, ammoRemain);
    }

    // 발사 시도
    public void Fire() 
    {
        if(state == State.Ready && Time.time > lastFireTime + gunData.timeBetFire)
        {
            lastFireTime = Time.time;
            Shot();
        }
    }

    [PunRPC]
    public void ShotOnServer()
    {
        var hitPoint = Vector3.zero;
        var ray = new Ray(fireTransform.position, fireTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, fireDistance))
        {
            hitPoint = hitInfo.point;
            //Damage
            var damagable = hitInfo.collider.GetComponent<IDamageable>();
            if (damagable != null)
            {
                damagable.OnDamage(gunData.damage, hitPoint, hitInfo.normal);
            }
        }
        else
        {
            hitPoint = fireTransform.position + fireTransform.forward * fireDistance;
        }
        photonView.RPC("ShotEffectOnClients", RpcTarget.All, hitPoint);
    }

    [PunRPC]
    public void ShotEffectOnClients(Vector3 hitPoint)
    {
        StartCoroutine(ShotEffect(hitPoint));
    }

    // 실제 발사 처리
    private void Shot() 
    {
        photonView.RPC("ShotOnServer", RpcTarget.MasterClient);
        --magAmmo;
        if(magAmmo == 0)
        {
            state = State.Empty;
        }
        UIManager.instance.UpdateAmmoText(magAmmo, ammoRemain);

        //StartCoroutine(ShotEffect(fireTransform.position + fireTransform.forward * 10f));
    }

    // 발사 이펙트와 소리를 재생하고 총알 궤적을 그린다
    private IEnumerator ShotEffect(Vector3 hitPosition) {

        // 라인 렌더러를 활성화하여 총알 궤적을 그린다

        bulletLineRenderer.SetPosition(0, fireTransform.position);
        bulletLineRenderer.SetPosition(1, hitPosition);
        bulletLineRenderer.enabled = true;

        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(gunData.shotClip);

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성화하여 총알 궤적을 지운다
        bulletLineRenderer.enabled = false;
    }

    // 재장전 시도
    public bool Reload()
    {
        Debug.Log("Reload");

        if(ammoRemain > 0 && magAmmo != gunData.magCapacity)
        {
            StartCoroutine(ReloadRoutine());
            return true;
        }
        return false;
    }

    // 실제 재장전 처리를 진행
    private IEnumerator ReloadRoutine() {

        state = State.Reloading;

        gunAudioPlayer.PlayOneShot(gunData.reloadClip);

        yield return new WaitForSeconds(gunData.reloadTime);

        magAmmo = gunData.magCapacity;
        ammoRemain -= magAmmo;

        UIManager.instance.UpdateAmmoText(magAmmo, ammoRemain);

        state = State.Ready;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ammoRemain);
            stream.SendNext(magAmmo);
            stream.SendNext(state);
        }
        else
        {
            ammoRemain = (int)stream.ReceiveNext();
            magAmmo = (int)stream.ReceiveNext();
            state = (State)stream.ReceiveNext();
        }
    }
}