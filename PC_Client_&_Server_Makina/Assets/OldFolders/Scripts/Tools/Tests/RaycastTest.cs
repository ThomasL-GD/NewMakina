using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(255,0,0,.5f);
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2f))
        {
            Debug.DrawLine(transform.position,hit.point, Color.green);
            Gizmos.DrawSphere(hit.point, .1f);
            
            Vector3 startPoint = hit.point + Vector3.up;
            Collider firstHitCollider = hit.collider;
            
            if(Physics.Raycast(startPoint, Vector3.down, out hit, 2f))
            {
                Debug.DrawLine(startPoint, hit.point, Color.green);
                Gizmos.DrawSphere(hit.point, .1f);
                if (firstHitCollider != hit.collider)
                {
                    
                    startPoint = transform.position + Vector3.up * (hit.point.y - transform.position.y);
                    if (Physics.Raycast(startPoint, transform.forward, out hit, 2f))
                    {
                        Debug.DrawLine(startPoint, hit.point, Color.green);
                        Gizmos.DrawSphere(hit.point, .1f);

                        startPoint = hit.point + Vector3.up;
                        if (Physics.Raycast(startPoint, Vector3.down, out hit, 2f))
                        {
                            Debug.DrawLine(startPoint, hit.point, Color.green);
                            Gizmos.DrawSphere(hit.point, .1f);
                        }

                        Collider[] capsuleHits = Physics.OverlapCapsule(hit.point + Vector3.up * .71f, hit.point + Vector3.up * 2f, .35f);

                        if (capsuleHits.Length < 1)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawWireSphere(hit.point + Vector3.up * .35f, .35f);
                            Gizmos.DrawWireSphere(hit.point + Vector3.up * 2, .53f);
                        }
                        else
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawWireSphere(hit.point + Vector3.up * .71f, .35f);
                            Gizmos.DrawWireSphere(hit.point + Vector3.up * 2, .35f);
                        }
                    }
                }else
                {
                    Collider[] capsuleHits = Physics.OverlapCapsule(hit.point + Vector3.up * .35f, hit.point + Vector3.up * 2f, .35f);
                    
                    if (capsuleHits.Length < 1)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(hit.point + Vector3.up * .35f, .35f);
                        Gizmos.DrawWireSphere(hit.point + Vector3.up * 2, .35f);
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(hit.point + Vector3.up * .35f, .35f);
                        Gizmos.DrawWireSphere(hit.point + Vector3.up * 2, .35f);
                    }
                }
            }
        }
        else
        {
            Debug.DrawRay(transform.position,transform.forward *2f, Color.red);
        }
    }
}
