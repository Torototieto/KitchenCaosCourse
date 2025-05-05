using UnityEngine;

public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        // To avoid the player from moving faster diagonally we need to
        // normalize the Vector2
        inputVector.Normalize();

        // Debug.Log(inputVector);

        return inputVector;
    }
}
