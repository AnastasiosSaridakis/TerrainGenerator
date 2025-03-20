using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraformingCamera : MonoBehaviour
{
    [SerializeField] private float brushSize = 2f;
    [SerializeField] private float terraformingStrength = 1f;
    private Vector3 hitpoint;
    private Camera camera;

    private void Awake()
    {
        camera = gameObject.GetComponent<Camera>();
    }

    private void Terraform(bool add)
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit,1000))
        {
            hitpoint = hit.point;
            
            List<Chunk> hitChunks = new List<Chunk>();
            RaycastHit[] sphereCastHits =
                Physics.SphereCastAll(camera.ScreenPointToRay(Input.mousePosition), brushSize, 1000);

            if (sphereCastHits.Length > 0)
            {
                foreach (var sphereCastHit in sphereCastHits)
                {
                    Chunk hitChunk = sphereCastHit.collider.gameObject.GetComponent<Chunk>();
                    if (!hitChunks.Contains(hitChunk))
                    {
                        hitChunks.Add(hitChunk);
                    }
                }
            }
            foreach (var chunk in hitChunks)
            {
                chunk.EditWeights(hitpoint, brushSize, add,terraformingStrength);
            }
        }
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Terraform(true);
        }else if (Input.GetMouseButton(1))
        {
            Terraform(false);
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hitpoint, brushSize);
    }
}
