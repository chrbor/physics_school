using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ManipulatorScript : MonoBehaviour
{
    private CircleCollider2D col;
    private float res;
    private bool block, charBlock;
    public bool dragBlock;
    private bool hovering;

    private ManipulatorMenu manipulatorMenu;
    private CharacterMenu characterMenu;
    public GameObject formulaBook;

    void Start()
    {
        col = GetComponent<CircleCollider2D>();
        res = (float)Screen.height / Screen.width;

        manipulatorMenu = GameObject.Find("Canvas").transform.Find("ManipulationMenu").GetComponent<ManipulatorMenu>();
        block = false;
        charBlock = false;
    }
    
    void Update()
    {
        //setze die Position des Colliders zu der des Mauszeigers und aktiviere ihn bei Mausdruck:
        col.offset = Camera.main.orthographicSize * 2 * new Vector3((Input.mousePosition.x / Screen.width -0.5f) * Camera.main.aspect, (Input.mousePosition.y / Screen.height -0.5f));

        if (Input.GetKey(KeyCode.F))
        {
            if (charBlock || block) return;
            charBlock = true;
            hovering = false;
            dragBlock = false;
            //öffne das Charaktermenü:
            Debug.Log("Ändere Char");
            GameObject obj = Instantiate(formulaBook);
            characterMenu = obj.GetComponent<CharacterMenu>();
            characterMenu.CreateMenu();
        }
        if(charBlock)
            col.enabled = true;
        else
            col.enabled = Input.GetMouseButton(0);
    }

    public void RemoveBlock() { block = false; charBlock = false; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (block || dragBlock) return;

        if((other.CompareTag("Object") && !charBlock) || other.CompareTag("Player") || other.CompareTag("Ally"))
        {
            if (charBlock)
            {
                if(other.name != "_2") if (!other.GetComponent<GroupScript>().inGroup) return;

                if (!hovering)
                    if (other.transform.parent != null)
                        StartCoroutine(RemoveUnit(other.gameObject));
                    else
                        StartCoroutine(DragUnit(other.gameObject));
            }
            else
            {
                block = true;
                //öffne das Manipulationsmenü:
                Debug.Log(manipulatorMenu);
                manipulatorMenu.CreateMenu(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!dragBlock)
            hovering = false;
    }

    IEnumerator DragUnit(GameObject unit)
    {
        hovering = true;
        yield return new WaitWhile(()=> !Input.GetMouseButton(0) && hovering);
        if (!hovering) yield break;

        dragBlock = true;
        while (Input.GetMouseButton(0))
        {
            unit.transform.position = (Vector2)transform.position + col.offset;
            yield return new WaitForEndOfFrame();
            characterMenu.UpdateSelection(unit);
        }
        characterMenu.AddUnit(unit);
        dragBlock = false;
        yield break;
    }

    IEnumerator RemoveUnit(GameObject unit)
    {
        hovering = true;
        yield return new WaitWhile(() => !Input.GetMouseButton(0) && hovering);
        if (!hovering) yield break;

        dragBlock = true;
        characterMenu.SubstractUnit(unit);
        yield return new WaitWhile(()=> Input.GetMouseButton(0));
        dragBlock = false;
        yield break;
    }
}

