using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public LayerMask battleLayer;
    private bool isMoving;

    private Vector2 input;

    private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        HandleMovement();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0)
            {
                input.y = 0;
            }

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }

        animator.SetBool("isMoving", isMoving);
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Overlaps(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }

    private Collider2D Overlaps(Vector3 targetPos, float radius, LayerMask layer)
    {
        return Physics2D.OverlapCircle(targetPos, radius, layer);
    }

    private void Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;
        var collider = Overlaps(interactPos, 0.2f, interactableLayer);
        if (collider)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    private void CheckForEncounters()
    {
        if (Overlaps(transform.position, 0.01f, battleLayer) != null)
        {
            if (Random.Range(1, 100) <= 50)
            {
                Debug.Log("Battle started");
            }
        }
    }
}
