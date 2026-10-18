using Player;
using UnityEngine;

namespace GTFO.CrashTestDummies;

/// <summary>Shared "can any player currently see this point" check, used to gate both spawning and cleanup.</summary>
internal static class Visibility
{
    private const float BodyHeight = 1f;

    /// <summary>
    /// True if no alive player currently has eyes on <paramref name="point"/>. A point outside every
    /// player's view cone is hidden outright; one inside a cone is still hidden if a line-of-sight check
    /// says something solid (a wall, a corner) is in the way, so spots just around a corner stay usable
    /// even close to the player.
    /// </summary>
    public static bool IsHiddenFromAllPlayers(Vector3 point, float viewHalfAngleDegrees)
    {
        var agents = PlayerManager.PlayerAgentsInLevel;
        if (agents == null)
            return true;

        for (var i = 0; i < agents.Count; i++)
        {
            var agent = agents[i];
            if (agent == null || !agent.Alive)
                continue;

            var cam = agent.FPSCamera;
            if (cam == null)
                continue;

            // Aim at body height, not the feet: a line to the feet grazes the floor at its end and
            // reads as blocked. Only walls and level geometry count as cover - the enemy's own
            // colliders (or another enemy's) don't hide it.
            var eye = agent.EyePosition;
            var target = point + Vector3.up * BodyHeight;
            var toPoint = target - eye;
            if (Vector3.Angle(cam.transform.forward, toPoint) > viewHalfAngleDegrees)
                continue;

            if (!Physics.Linecast(eye, target, LayerManager.MASK_WORLD))
                return false;
        }

        return true;
    }
}
