using UnityEngine;
using Photon.Pun;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviourPun {
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터

    private void Awake() 
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        playerAnimator.SetFloat("Move", playerInput.move);

    }


    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate() 
    {

        if(!photonView.IsMine)
        {
            return;
        }
        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행
        Rotate();
        Move();
    }

    // 입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move() 
    {
        var pos = playerRigidbody.position;
        pos += transform.forward * playerInput.move * moveSpeed * Time.deltaTime;
        playerRigidbody.MovePosition(pos);

    }

    // 입력값에 따라 캐릭터를 좌우로 회전
    private void Rotate() 
    {
        var rot = playerRigidbody.rotation;
        rot *= Quaternion.Euler(0f, playerInput.rotate * rotateSpeed * Time.deltaTime, 0f);
        playerRigidbody.MoveRotation(rot);

    }
}