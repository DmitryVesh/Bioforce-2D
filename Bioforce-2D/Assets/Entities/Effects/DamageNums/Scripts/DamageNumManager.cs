using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;

public class DamageNumManager : MonoBehaviour
{
    public static DamageNumManager Instance { get => instance; }
    private static DamageNumManager instance;

    [SerializeField] private GameObject DamageNumPrefab; //Set in inspector
    private List<DamageNum> DamageNumList = new List<DamageNum>();

    public void Create(Vector3 position, int damage, bool isGoingRight)
    {
        DamageNum damageNum;
        damageNum = GetDamageNum();

        if (damageNum == null)
            damageNum = AddDamageNum();

        damageNum.Activate(position, damage, isGoingRight);
    }

   
    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }

    private DamageNum AddDamageNum()
    {
        GameObject damageNumGameObject = Instantiate(DamageNumPrefab, Vector3.zero, Quaternion.identity, transform);
        DamageNum damageNumScript = damageNumGameObject.GetComponent<DamageNum>();

        DamageNumList.Add(damageNumScript);
        return damageNumScript;
    }
    private DamageNum GetDamageNum()
    {
        DamageNum damageNum = null;
        foreach (DamageNum popup in DamageNumList)
        {
            if (popup.IsAvailable())
            {
                damageNum = popup;
                break;
            }
        }
        return damageNum;
    }
    
    private void OnDestroy()
    {
        foreach (DamageNum damageNum in DamageNumList)
        {
            if (damageNum == null)
                continue;
            Destroy(damageNum.gameObject);
        }
    }
}
