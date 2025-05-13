using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
   // Singleton pattern
   public static Player Instance { get; private set; }
   public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
   public class OnSelectedCounterChangedEventArgs : EventArgs
   {
      public BaseCounter selectedCounter;
   }
   [SerializeField] private float moveSpeed = 7f;
   [SerializeField] private GameInput gameInput;
   // variable to set the Layer Mask of the counters so that the Raycast
   // from HandleInteractions only hits objects in that layer
   [SerializeField] private LayerMask countersLayerMask;
   [SerializeField] private Transform KitchenObjectHoldPoint;

   private bool isWalking;
   private Vector3 lastInteractDir;
   private BaseCounter selectedCounter;
   private KitchenObject kitchenObject;

   private void Awake()
   {
      if (Instance != null)
      {
         Debug.LogError("There is more than one Player instance");
      }
      Instance = this;
   }

   private void Start()
   {
      gameInput.OnInteractAction += GameInput_OnInteractAction;
   }

   private void GameInput_OnInteractAction(object sender, System.EventArgs e)
   {
      if (selectedCounter != null)
      {
         selectedCounter.Interact(this);
      }
   }

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
            // can move only on the XOnInteractActionX
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
      // countersLayerMask is so it only returns objects on that layer
      if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
      {
         if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
         {
            // Has ClearCounter
            if (baseCounter != selectedCounter)
            {
               SetSelectedCounter(baseCounter);
            }
         }
         else
         {
            SetSelectedCounter(null);
         }
      }
      else
      {
         SetSelectedCounter(null);
      }

   }

   private void SetSelectedCounter(BaseCounter selectedCounter)
   {
      this.selectedCounter = selectedCounter;

      OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
      {
         selectedCounter = selectedCounter
      });
   }

   public Transform GetKitchenObjectFollowTransform()
   {
      return KitchenObjectHoldPoint;
   }

   public void SetKitchenObject(KitchenObject kitchenObject)
   {
      this.kitchenObject = kitchenObject;
   }

   public KitchenObject GetKitchenObject()
   {
      return kitchenObject;
   }

   public void ClearKitchenObject()
   {
      kitchenObject = null;
   }

   public bool HasKitchenObject()
   {
      return kitchenObject != null;
   }
}