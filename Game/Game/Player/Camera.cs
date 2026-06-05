using OpenTK.Mathematics;
using Engine.Core.GameObjects;
using Engine.Core.Rendering;
using Engine.Core.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Engine.Core;

namespace TowerDefecse
{
    public class CameraMover : Component
    {
        private float speedCamera = 3.5f;
        public Mover player = null!;
        public Camera camera = null!;
        private bool isMoved;
        private float cameraMultiplier = 1.5f;

        public override void Update(float dt)
        {
            if(player == null || camera == null) throw new Exception("Player or Camera is not assigned in CameraMover component.");
            if(isMoved || (player.Transform.Position - camera.Position).Length > 1f)
            {
                Vector2 playerDirection = player.GetDirection();
                Vector2 cameraTarget = player.Transform.Position + playerDirection * cameraMultiplier; // Offset in the direction of movement
                camera.Position = Vector2.Lerp(camera.Position, cameraTarget, speedCamera * dt);
                isMoved = true;
            }
            if(isMoved && player.GetDirection() == Vector2.Zero)
            {
                if ((player.Transform.Position - camera.Position).Length > 0.01f)
                {
                    Vector2 playerDirection = player.GetDirection();
                    Vector2 cameraTarget = player.Transform.Position + playerDirection * cameraMultiplier; // Offset in the direction of movement
                    camera.Position = Vector2.Lerp(camera.Position, cameraTarget, speedCamera * dt / 3f); // Slow down when player stops
                }
                else
                {
                    camera.Position = player.Transform.Position;
                    isMoved = false;
                }
            }

        }
    }
}