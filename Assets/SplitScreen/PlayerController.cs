using UnityEngine;
using UnityEngine.InputSystem;

namespace SplitScreen
{
    /// <summary>
    /// Allows a player to use an input control scheme to move the object.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// The name of the keyboard control scheme to use for this controller.
        /// </summary>
        [SerializeField]
        private string controlScheme;

        /// <summary>
        /// The input action that can be used to move the object.
        /// </summary>
        private InputAction moveAction;

        /// <summary>
        /// The prefab to use for creating player input components.
        /// </summary>
        [SerializeField]
        private GameObject playerInputPrefab;

        /// <summary>
        /// Handles any logic specific to this behaviour that should be executed once, after the
        /// object is created and activated but before any updates have occurred.
        /// </summary>
        private void Start()
        {
            // Create a player input using the specified control scheme.
            PlayerInput playerInput = PlayerInput.Instantiate(
                this.playerInputPrefab,
                -1,
                this.controlScheme,
                -1,
                Keyboard.current);
            moveAction = playerInput.actions.FindAction("Move");
            playerInput.transform.parent = this.transform;
        }

        /// <summary>
        /// Handles any logic that should be executed on each frame while the behaviour is active.
        /// </summary>
        private void Update()
        {
            // Move the object in the direction it's facing when the input is pressed.
            const float MovementSpeed = 5f;
            Vector2 moveValue = moveAction.ReadValue<Vector2>();
            Vector3 moveValue3d = new Vector3(
                moveValue.x * MovementSpeed * Time.deltaTime,
                0,
                moveValue.y * MovementSpeed * Time.deltaTime);
            moveValue3d = this.transform.rotation * moveValue3d;

            this.transform.position += moveValue3d;
        }
    }
}