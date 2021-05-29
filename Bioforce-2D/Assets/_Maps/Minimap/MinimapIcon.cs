using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    [SerializeField] public Image Icon;

    [SerializeField] public Sprite IconWithinRange;
    [SerializeField] public Sprite IconOutOfRange;
    
    [SerializeField] public bool ShouldClamp;
    [SerializeField] public Color Color;

    [SerializeField] public int SortOrder = 0; //Larger values get placed on top of smaller values

    protected virtual void Start()
    {
        Minimap.SubscribeIcon(this, Color);
    }

    protected virtual void OnDestroy()
    {
        Minimap.Unsubscribe(this);

        if (!Icon || !Icon.gameObject)
            return;

        Destroy(Icon.gameObject);
        //if (!(Icon.gameObject is null))
        //    Destroy(Icon.gameObject);

    }
}
