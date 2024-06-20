using UnityEngine;

public class PlayerPowerUp : MonoBehaviour
{
    private void OnEnable()
    {
        PowerUpJump.OnPoweUpCollected += AddDoubleJump;
    }

    private void OnDisable()
    {
        PowerUpJump.OnPoweUpCollected -= AddDoubleJump;
    }
    
    private void AddDoubleJump(PowerUpJump powerUpJump)
    {
        GetComponent<PlayerJump>().doubleJump = true;
    }
}
