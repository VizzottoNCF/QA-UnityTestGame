using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{

    private InputAction _shootAction;
    public static bool shootWasPressed;
    public static bool shootWasReleased;

    [Header("Shoot Vars")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _shootCooldown;
    [SerializeField] private float _projectileSpeed;
    private float _shootTimer = 0f;

    private void Start()
    {
        _shootAction = Movement.PlayerInput.actions["Shoot"];
    }

    private void Update()
    {
        if (!Movement.gameHasStarted) { return; }

        shootWasPressed = _shootAction.WasPressedThisFrame();
        shootWasReleased = _shootAction.WasReleasedThisFrame();


        // shoot actions
        if (_shootTimer <= 0)
        {
            if (shootWasPressed)
            {

                Vector2 direction = transform.right * Mathf.Sign(transform.localScale.x);
                Vector2 position = (Vector2)transform.position + direction * 0.5f; // offset spawn point a bit
                GameObject projectile = Instantiate(_projectilePrefab, position, Quaternion.identity);
                projectile.GetComponent<Projectile>().Fire(_projectileSpeed, direction);

                _shootTimer = _shootCooldown;
            }
        }
        //shoot cooldown timer
        else
        {
            _shootTimer -= Time.deltaTime;
        }
    }
}
