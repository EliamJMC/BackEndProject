/*
    [Header] es para crear un encabezado en el inspector de Unity.
    [SerializeField] es para hacer que una variable privada sea visible y editable en el inspector de Unity.

    private es para declarar una variable que solo puede ser accedida dentro de la misma clase.
    public es para declarar una variable que puede ser accedida desde cualquier otra clase.
*/
using System.Runtime.CompilerServices;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    // Gravity con
    public const float Gravity = -9.81f;

    
    [Header("Variables")]
    // Speed vars
    public float courrentSpeed, walkingSpeed = 5.0f, runningSpeed;
    public float jumpForce = 0.5f;
    public float rotation = 10f;

    // State
    [Header("Estados")]
    [SerializeField]
    private Vector3 velocity;

    // States
    [Header("States")]
    public bool isGrounded;
    public bool isMoving;
    public bool isRunning;
    public bool isJumping;

    // Components
    [Header("Componentes")]
    [SerializeField]    private CharacterController controller;
    [SerializeField]    private Animator animator;
    [SerializeField]    private Transform camTransform;
    [SerializeField]    private CharacterStats characterStats;
    [SerializeField]    private Timer timer = new Timer(); // Create a new "Timer" script


    void Start()
    {
        courrentSpeed = walkingSpeed;
        runningSpeed = walkingSpeed * 1.5f;

        characterStats = GetComponent<CharacterStats>();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        camTransform = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        GravityManagemnt();
        UpdatingMethods(move);
        Mouvement(move);

    }
    void UpdatingMethods(Vector3 move)
    {
        timer.Update(Time.deltaTime);

        isGrounded = controller.isGrounded;
        isMoving = move.magnitude >= 0.1f;
        isRunning = Input.GetKey(KeyCode.LeftShift) && isGrounded && characterStats.currentStamina > 0;
        isJumping = Input.GetButtonDown("Jump") && isGrounded;
    }
    
    void Mouvement(Vector3 move)
    {
        // Basic moves
        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(move.normalized.x, move.normalized.z) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotation, 0.1f);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0, angle, 0);
            controller.Move(moveDir.normalized * courrentSpeed * Time.deltaTime);

            animator.SetFloat("Speed", move.magnitude);

            // Running logic
            if (isRunning)
            {
                courrentSpeed = runningSpeed;
                animator.SetBool("isRunning", true);

                // Jumping while running
                if (isJumping)
                    animator.SetTrigger("Jump");
            }
            else
            {
                courrentSpeed = walkingSpeed;
                animator.SetBool("isRunning", false);
            }
        } 
        // Idle Jump
        else if (isJumping)
        {
            timer.StartTimer(0.6f);
            timer.OnTimerComplete += Impultion;
            animator.SetTrigger("Jump");
        }


    }

    void GravityManagemnt() 
    {
        //Dont touch I dont know how it works
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += Gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Can be upgraded and should
    void Impultion() { velocity.y = Mathf.Sqrt(jumpForce * -1f * Gravity); }
}
