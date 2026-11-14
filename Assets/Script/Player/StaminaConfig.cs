using UnityEngine;

[CreateAssetMenu(fileName = "StaminaConfig", menuName = "Game/Stamina Config")]
public class StaminaConfig : ScriptableObject
{
    [Min(1f)] public float maximumStamina = 100f;
    [Min(0f)] public float cookingDrainPerGameMinute = 1f;
    [Min(0f)] public float shippingDrainPerGameMinute = 2f;
    [Min(0f)] public float restingRecoveryPerGameMinute = 2f;
    [Min(0f)] public float sleepingRecoveryPerGameMinute = 8f;
    [Min(1f)] public float sleepTimeMultiplier = 8f;
}
