using OpenTK.Mathematics;
using Engine.Core.GameObjects;
using Engine.Core.Input;
using Engine.Core.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Engine.Core;

namespace TowerDefecse
{
    public class Mover : Component
    {
        private float durationDash = 0.1f;
        private float currentDashTime = 0f;
        private float speed = 4;
        private float dashSpeed = 20f;
        private float DashCooldown = 3f;
        private float currentDashCooldown = 0f;
        public TileChunk chunk = null!;
        private Vector2 dashDirection = Vector2.Zero;
        private Vector2 movementDirection = Vector2.Zero;
        private Vector2 Direction = Vector2.Zero;
        public Vector2 GetDirection() => Direction;
        public override void Update(float dt)
        {
            if (Input.GetKey(Keys.W))
            {
                movementDirection += new Vector2(0, 1f);
            }
            if (Input.GetKey(Keys.S))
            {
                movementDirection += new Vector2(0, -1f);
            }
            if (Input.GetKey(Keys.A))
            {
                movementDirection += new Vector2(-1f, 0);
            }
            if (Input.GetKey(Keys.D))
            {
                movementDirection += new Vector2(1f, 0);
            }
            if (Input.GetKeyDown(Keys.LeftShift) && currentDashCooldown <= 0f)
            {
                currentDashTime = durationDash;
                currentDashCooldown = DashCooldown;
            }
            if (currentDashCooldown > 0f)
            {
                currentDashCooldown -= dt;
            }
            if (movementDirection != Vector2.Zero)
            {
                Vector2 direction = movementDirection.Normalized();
                if (durationDash > 0f)
                {
                    dashDirection = direction;
                }

                // Выбираем текущую скорость и направление
                Vector2 activeDir = currentDashTime > 0f ? dashDirection : direction;
                float activeSpeed = currentDashTime > 0f ? dashSpeed : speed;
                Vector2 moveAmount = activeDir * activeSpeed * Time.DeltaTime;

                float radius = 0.4f; // "Толщина" игрока. 0.4 означает, что коллизия 0.8 x 0.8

                // Пытаемся двигаться по X
                if (moveAmount.X != 0f)
                {
                    float nextX = Transform.Position.X + moveAmount.X;
                    float checkX = nextX + Math.Sign(moveAmount.X) * radius;

                    // Проверяем верхний и нижний углы ведущего ребра по X
                    bool xCollision = chunk.IsSolidAt(checkX, Transform.Position.Y - radius) ||
                                      chunk.IsSolidAt(checkX, Transform.Position.Y + radius);

                    if (!xCollision)
                    {
                        Transform.Position = new Vector2(nextX, Transform.Position.Y);
                    }
                }

                // Пытаемся двигаться по Y
                if (moveAmount.Y != 0f)
                {
                    float nextY = Transform.Position.Y + moveAmount.Y;
                    float checkY = nextY + Math.Sign(moveAmount.Y) * radius;

                    // Проверяем левый и правый углы ведущего ребра по Y
                    bool yCollision = chunk.IsSolidAt(Transform.Position.X - radius, checkY) ||
                                      chunk.IsSolidAt(Transform.Position.X + radius, checkY);

                    if (!yCollision)
                    {
                        Transform.Position = new Vector2(Transform.Position.X, nextY);
                    }
                }

                if (currentDashTime > 0f)
                {
                    currentDashTime -= Time.DeltaTime;
                }
            }

            Direction = movementDirection;
            movementDirection = Vector2.Zero;
            
        }
    }
}