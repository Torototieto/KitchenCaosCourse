using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 7f;
   [SerializeField] private GameInput gameInput;

   private bool isWalking;

   // Update is called once per frame
   private void Update()
   {
      Vector2 inputVector = gameInput.GetMovementVectorNormalized();

      // Since inputVector is a Vector2, we need to convert it to a 
      // Vector3 so we can move on the right axis
      Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
      transform.position += moveDir * moveSpeed * Time.deltaTime;

      // Rotate the Player to go forward and smooth with Slerp
      float rotateSpeed = 10f;
      transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);

      isWalking = (moveDir == Vector3.zero) ? false : true;

      Debug.Log(inputVector);
   }
   public bool IsWalking()
   {
      return isWalking;
   }
}