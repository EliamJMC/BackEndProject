/*
    [Header] es para crear un encabezado en el inspector de Unity.
    [SerializeField] es para hacer que una variable privada sea visible y editable en el inspector de Unity.

    private es para declarar una variable que solo puede ser accedida dentro de la misma clase.
    public es para declarar una variable que puede ser accedida desde cualquier otra clase.
*/
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    // Constants
    public const float Gravity = -9.81f;

    // Variables
    [Header("Variables")]
    public float speed = 5f;
    public float jumpForce = 2f;
    public float rotationSpeed = 10f;

    // State
    [Header("Estados")]
    [SerializeField]
    private Vector3 velocity;
    [SerializeField]
    private bool isGrounded;

    // Components
    [Header("Componentes")]
    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private Animator animator;
    private Transform camTransform;

    // *NOTA* En este void, se guardan todas las instrucciones que se ejecutan SOLO al iniciarse el programa.
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        camTransform = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // *NOTA* En este void, se guardan todas las instrucciones que se ejecutan CADA FRAME.
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
            float targetAngle = Mathf.Atan2(moveNormalized.x, moveNormalized.z) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);

            // Rotar el personaje hacia la dirección del movimiento
            transform.rotation = Quaternion.Euler(0, angle, 0);

            // Mover el personaje
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Gravity);
            animator.SetTrigger("Jump");
        }

        // Aplicar gravedad
        velocity.y += Gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
