using System.Collections.Generic;
using UnityEngine;

public class DamageNumManager : MonoBehaviour
{
    public static DamageNumManager Instance;
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
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"DamageNumManager instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
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
