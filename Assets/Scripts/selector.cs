using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selector : MonoBehaviour
{
    Collider m_Collider;
    public List<GameObject> hitboxes;
    public bool isIntersecting;
    [SerializeField]
    private LayerMask selectorLayermask;

    // Start is called before the first frame update
    void Start()
    {
        m_Collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateHitbox(GameObject hitbox)
    {
        if (!hitboxes.Contains(hitbox))
            this.hitboxes.Add(hitbox);
    }
}
