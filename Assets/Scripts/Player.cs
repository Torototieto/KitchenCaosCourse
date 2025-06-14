using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
   // Singleton pattern
   public static Player Instance { get; private set; }

   public event EventHandler OnPickedSomething;
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
      gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
   }

   private void GameInput_OnInteractAction(object sender, System.EventArgs e)
   {
      if (GameManager.Instance.IsGamePlaying())
      {
         if (selectedCounter != null)
         {
            selectedCounter.Interact(this);
         }
      }
   }

   private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e)
   {
      if (GameManager.Instance.IsGamePlaying())
      {
         if (selectedCounter != null)
         {
            selectedCounter.InteractAlternate(this);
         }
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

      float moveDistance = moveSpeed * Time.deltaTime;
      float playerRadius = 0.7f;
      float playerHeight = 2.0f;

      // Collision logic
      bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
          playerRadius, moveDir, moveDistance);

      if (!canMove)
      {
         // Cannot move towards moveDir

         // Attempt only X movement
         Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
         canMove = (moveDir.x < -0.5f || moveDir.x > 0.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
             playerRadius, moveDirX, moveDistance);

         if (canMove)
         {
            // Can move only on the X
            moveDir = moveDirX; // Use the X-axis direction
         }
         else
         {
            // Cannot move only on the X

            // Attempt only Z movement
            Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
            canMove = (moveDir.z < -0.5f || moveDir.z > 0.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
                playerRadius, moveDirZ, moveDistance);

            if (canMove)
            {
               // Can move only on the Z
               moveDir = moveDirZ;
            }
            else
            {
               // Cannot move in any direction
            }
         }
      }

      if (canMove)
      {
         // Apply movement
         transform.position += moveDir * moveDistance;

         // Rotate the Player to go forward and smooth with Slerp
         float rotateSpeed = 10f;
         transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);

         isWalking = (moveDir == Vector3.zero) ? false : true;
      }
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

      if (kitchenObject != null)
      {
         OnPickedSomething?.Invoke(this, EventArgs.Empty);
      }
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