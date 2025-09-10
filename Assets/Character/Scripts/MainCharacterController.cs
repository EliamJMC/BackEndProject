using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    // Constants
    public const float Gravity = -9.81f;

    // Variables
    public float Speed = 5f;
    public float JumpForce = 7f;
    public float rotationSpeed = 10f;

    // State
    private Vector3 velocity;
    private bool isGrounded;

    // Components
    private CharacterController controller;
    private Animator animator;

    // *NOTA* En este void, se guardan todas las instrucciones que se ejecutan SOLO al iniciarse el programa.
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            // Un pequeño ajuste para mantener al personaje pegado al suelo.
            velocity.y = -2f; 
        }

        // Movimiento en el plano XZ
        // Obtener entrada del usuario WASD o flechas
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Crear un vector de movimiento basado en la entrada
        Vector3 move = new Vector3(moveX, 0, moveZ);
        float moveMagnitude = move.magnitude;
        animator.SetFloat("Speed", move.magnitude);

        // Normalizar el vector de movimiento para evitar velocidad diagonal más rápida
        Vector3 moveNormalized = move.normalized;


        // Mover el personaje solo si hay entrada significativa
        if (move.magnitude >= 0.1f)
        {
            // Calcular la dirección del movimiento en función de la cámara
            float targetAngle = Mathf.Atan2(moveNormalized.x, moveNormalized.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);

            // Rotar el personaje hacia la dirección del movimiento
            transform.rotation = Quaternion.Euler(0, angle, 0);

            // Mover el personaje
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir.normalized * Speed * Time.deltaTime);
        }

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
            animator.SetTrigger("Jump");
        }

        // Aplicar gravedad
        velocity.y += Gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
