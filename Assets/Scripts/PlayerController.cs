using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Carriles")]
    [SerializeField] private float xCentro = 0f;
    [SerializeField] private float separacionCarril = 2.2f;   
    [SerializeField] private int carrilActual = 1;            

    [Header("Movimiento Horizontal (solo física)")]
    [SerializeField] private float velMaxX = 12f;             
    [SerializeField] private float acelMaxX = 60f;            
    [SerializeField] private float snapDist = 0.02f;          

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 14f;         
    [SerializeField] private Transform puntoSuelo;
    [SerializeField] private float radioSuelo = 0.15f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Swipe (móvil)")]
    [SerializeField] private float umbralSwipe = 75f;         
    [SerializeField] private float umbralSwipeVertical = 85f;

    [Header("Animator")]
    [SerializeField] Animator _anim;

    private Rigidbody2D rb;
    private bool tocando;
    private Vector2 toqueInicio;
    bool saltando;

    private float XDestino => carrilActual switch
    {
        0 => xCentro - separacionCarril,
        1 => xCentro,
        _ => xCentro + separacionCarril
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (!GameManager.Instancia.juegoActivo) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame) CambiarCarril(-1);
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) CambiarCarril(+1);
            if (Keyboard.current.spaceKey.wasPressedThisFrame) IntentarSaltar();
        }
        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.left.wasPressedThisFrame) CambiarCarril(-1);
            if (Gamepad.current.dpad.right.wasPressedThisFrame) CambiarCarril(+1);
            if (Gamepad.current.buttonSouth.wasPressedThisFrame) IntentarSaltar(); // A/X
        }

        var ts = Touchscreen.current;
        if (ts != null)
        {
            var touch = ts.primaryTouch;
            if (touch.press.isPressed && !tocando)
            {
                tocando = true;
                toqueInicio = touch.position.ReadValue();
            }
            else if (!touch.press.isPressed && tocando)
            {
                tocando = false;
                var fin = touch.position.ReadValue();
                var delta = fin - toqueInicio;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    if (Mathf.Abs(delta.x) >= umbralSwipe) CambiarCarril(Mathf.Sign(delta.x) > 0 ? +1 : -1);
                }
                else
                {
                    if (delta.y >= umbralSwipeVertical) IntentarSaltar();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instancia.juegoActivo) return;
        MoverHaciaCarrilPorFisica();
    }

    void MoverHaciaCarrilPorFisica()
    {
        float x = rb.position.x;
        float dx = XDestino - x;
        float vx = rb.linearVelocity.x;

        float distanciaFrenado = (vx * vx) / (2f * Mathf.Max(0.0001f, acelMaxX));
        float acc; 

        if (Mathf.Abs(dx) <= snapDist && Mathf.Abs(vx) < 0.1f)
        {
            var v = rb.linearVelocity;
            v.x = 0f;
            rb.linearVelocity = v;
            return;
        }

        if (Mathf.Abs(dx) > distanciaFrenado)
            acc = Mathf.Sign(dx) * acelMaxX;         
        else
            acc = -Mathf.Sign(vx) * acelMaxX;        

        rb.AddForce(new Vector2(acc * rb.mass, 0f), ForceMode2D.Force);
        var vel = rb.linearVelocity;
        vel.x = Mathf.Clamp(vel.x, -velMaxX, velMaxX);
        rb.linearVelocity = vel;
    }

    void CambiarCarril(int dir)
    {
        carrilActual = Mathf.Clamp(carrilActual + dir, 0, 2);
    }

    void IntentarSaltar()
    {
        if (!EnSuelo())
        {
            return;
        }
        saltando = true;
        _anim.SetTrigger("Jumping");
        var v = rb.linearVelocity;
        v.y = 0f;                         
        rb.linearVelocity = v;
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
    }

    bool EnSuelo()
    {
        return Physics2D.OverlapCircle(puntoSuelo.position, radioSuelo, capaSuelo) != null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstaculo"))
        {
            if(EnSuelo() == true)
            {
                GameManager.Instancia.GameOver();
            }
            else
            {
                FindFirstObjectByType<GameManager>().AddPoints(30);
                //Aqui se agregan puntos cuando saltan enemigos por si quieren cambiarlo a otra cosa chiques
            }
        }
            
    }

    void OnDrawGizmosSelected()
    {
        if (puntoSuelo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(puntoSuelo.position, radioSuelo);
        }
    }
}
