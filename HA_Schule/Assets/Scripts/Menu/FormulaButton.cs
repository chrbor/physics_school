using UnityEngine;
using UnityEngine.Events;

public class FormulaButton : MonoBehaviour
{
    [System.Serializable]
    public class OnEvent : UnityEvent{ };

    private SpriteRenderer sprite;
    public Sprite idle;
    public Sprite hover;
    public Sprite press;

    public OnEvent ButtonPressed;
    private bool hovering, block, externBlock;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        externBlock = false;
        hovering = false;
    }

    public void SetBlock(bool block)
    {
        externBlock = block;
    }

    private void Update()
    {
        if (externBlock) return;

        if (hovering)
        {
            if (Input.GetMouseButton(0))
            {
                if (press) sprite.sprite = press; else sprite.color = new Color(0.6f, 0.6f, 0.6f, 1);
                if (!block)
                {
                    block = true;
                    ButtonPressed.Invoke();
                }
            }
            else if (hover) sprite.sprite = hover; else sprite.color = new Color(0.8f, 0.8f, 0.8f, 1);
        }
        else { if (idle) sprite.sprite = idle; else sprite.color = Color.white; block = false; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        hovering = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //sprite.sprite = idle;
        hovering = false;
        block = false;
    }
}
