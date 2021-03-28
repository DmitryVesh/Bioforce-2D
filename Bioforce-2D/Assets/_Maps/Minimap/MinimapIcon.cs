using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    [SerializeField] public Image Icon;

    [SerializeField] public Sprite IconWithinRange;
    [SerializeField] public Sprite IconOutOfRange;
    
    [SerializeField] public bool ShouldClamp;
    public Color Color { get; set; }

    private void Start()
    {
        Minimap.SubscribeIcon(this, Color);
    }

    private void OnDestroy()
    {
        Minimap.Unsubscribe(this);

        if (!(Icon is null))
            Destroy(Icon);
        //if (!(Icon.gameObject is null))
        //    Destroy(Icon.gameObject);

    }
}
