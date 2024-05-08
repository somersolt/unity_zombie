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
        // 1. ���콺 ��ũ�� ��ǥ
        // 2. ���� (ī�޶�, ���콺�� ��ũ�� ��ǥ)
        // 3. ���� ĳ��Ʈ
        // 4. ���� ��ǥ

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
