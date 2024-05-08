using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavTest : MonoBehaviour
{
    public LayerMask layers;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
    }

    private void Update()
    {
        // 1. 마우스 스크린 좌표
        // 2. 레이 (카메라, 마우스의 스크린 좌표)
        // 3. 레이 캐스트
        // 4. 월드 좌표

        if(Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layers))
            {
                agent.SetDestination(hit.point);

            }

        }



    }
}
