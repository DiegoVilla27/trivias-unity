namespace Trivias.Player.Logic
{
    /// <summary>Velocidad a la que quiere ir el concursante: quieto, andando o corriendo (Shift).</summary>
    public static class MoveSpeedRules
    {
        public static float Target(bool hasInput, bool sprinting, float walkSpeed, float sprintSpeed)
        {
            if (!hasInput)
                return 0f;
            return sprinting ? sprintSpeed : walkSpeed;
        }
    }
}
