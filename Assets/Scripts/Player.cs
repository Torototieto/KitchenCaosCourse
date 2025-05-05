using System.Runtime.CompilerServices;
using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 7f;
   [SerializeField] private GameInput gameInput;
   // variable to set the Layer Mask of the counters so that the Raycast
   // from HandleInteractions only hits objects in that layer
   [SerializeField] private LayerMask countersLayerMask;

   private bool isWalking;
   private Vector3 lastInteractDir;

   // Update is called once per frame
   private void Update()
   {
      HandleMovement();
      HandleInteractions();
   }
   public bool IsWalking()
   {
      return isWalking;
   }

   private void HandleMovement()
   {
      Vector2 inputVector = gameInput.GetMovementVectorNormalized();

      // Since inputVector is a Vector2, we need to convert it to a 
      // Vector3 so we can move on the right axis
      Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

      // Collision logic so the player stops when it hits an object
      float MoveDistance = moveSpeed * Time.deltaTime;
      float playerRadius = 0.7f;
      float playerHeight = 2.0f;
      // Boolean that creates sort of a capsulle collider
      bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
         playerRadius, moveDir, MoveDistance);

      if (!canMove)
      {
         // cannot move towards moveDir (there is something in the way)
         // Attempt only movement on X axis
         Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
         canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
            playerRadius, moveDirX, MoveDistance);

         if (canMove)
         {
            // can move only on the X
            moveDir = moveDirX;
         }
         else
         {
            // cannot move only on the X
            // Attempt movement only on the Z axis
            Vector3 moveDirZ = new Vector3(moveDir.z, 0, 0).normalized;
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
               playerRadius, moveDirZ, MoveDistance);

            if (canMove)
            {
               // can move only on the Z
               moveDir = moveDirZ;
            }
         }

      }

      if (canMove)
      {
         transform.position += moveDir * MoveDistance;

      }

      // Rotate the Player to go forward and smooth with Slerp
      float rotateSpeed = 10f;
      transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);

      isWalking = (moveDir == Vector3.zero) ? false : true;
   }

   private void HandleInteractions()
   {
      Vector2 inputVector = gameInput.GetMovementVectorNormalized();
      Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y); 
      float interactDistance = 2f;

      if (moveDir != Vector3.zero)
      {
         lastInteractDir = moveDir;
      }

      // Uses and overload method that returns the object hit (out RaycastHit)
      if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
      {
         if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
         {
            // Has ClearCounter
            clearCounter.Interact();
         }
      } 
   }
}