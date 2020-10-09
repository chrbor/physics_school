using UnityEngine;

public class OV_Sorter : MonoBehaviour
{
    private SpriteRenderer sprite;
    private bool isObject;
    [HideInInspector]
    public bool sit;
    [HideInInspector]
    public bool up, down;
    // Start is called before the first frame update
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        isObject = CompareTag("Object");
    }

    // Update is called once per frame
    void Update()
    {
        if(isObject)
            sprite.sortingOrder = 10000 - (int)(transform.position.y*10);
        else
        {
            if(sit)
            {
                if(up)
                    sprite.sortingOrder = 10029 - (int)(transform.position.y*10);
                else
                    sprite.sortingOrder = 10018 - (int)(transform.position.y*10);
            }
            else
                sprite.sortingOrder = 10015 - (int)(transform.position.y*10);
        }
    }
}
