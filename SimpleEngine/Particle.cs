using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace CPI411.SimpleEngine
{
    public class Particle
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Acceleration { get; set; }
        public float Age { get; set; }
        public float MaxAge { get; set; }
        public Vector3 Color { get; set; }
        public float Size { get; set; }
        public float SizeVelocity { get; set; }
        public float SizeAcceleration { get; set; }

        // ── Physics extensions ──────────────────────────────────────
        /// <summary>Bounciness coefficient [0 = no bounce, 1 = perfect bounce].</summary>
        public float Resilience { get; set; } = 0.6f;
        /// <summary>Horizontal speed retained after each bounce [0..1].</summary>
        public float Friction { get; set; } = 0.8f;
        /// <summary>Maximum floor bounces before the particle sticks.</summary>
        public int MaxBounces { get; set; } = 0;
        /// <summary>Counts bounces so far this lifetime.</summary>
        public int BounceCount { get; set; } = 0;
        /// <summary>World-space Y level of the floor.</summary>
        public float FloorY { get; set; } = 0f;
        /// <summary>When true, Acceleration is refreshed externally each frame (gravity + wind).</summary>
        public bool UseGravity { get; set; } = false;

        public Particle() { Age = -1; }

        public bool Update(float ElapsedGameTime)
        {
            if (Age < 0) return false;

            Velocity += Acceleration * ElapsedGameTime;
            Position += Velocity * ElapsedGameTime;
            SizeVelocity += SizeAcceleration * ElapsedGameTime;
            Size += SizeVelocity * ElapsedGameTime;
            Age += ElapsedGameTime;

            // ── Floor bounce ─────────────────────────────────────────
            if (Position.Y < FloorY && Velocity.Y < 0)
            {
                Position = new Vector3(Position.X, FloorY, Position.Z);

                if (BounceCount < MaxBounces)
                {
                    Velocity = new Vector3(
                        Velocity.X * Friction,
                        -Velocity.Y * Resilience,
                        Velocity.Z * Friction);
                    BounceCount++;
                }
                else
                {
                    // Particle sticks — bleed off horizontal velocity via friction
                    Velocity = new Vector3(Velocity.X * Friction * 0.5f, 0f, Velocity.Z * Friction * 0.5f);
                }
            }

            if (Age > MaxAge) { Age = -1; return false; }
            return true;
        }

        public bool IsActive() => Age >= 0;
        public void Activate() { Age = 0; }

        public void Init()
        {
            Age = 0;
            Size = 1;
            SizeVelocity = 0;
            SizeAcceleration = 0;
            BounceCount = 0;
        }
    }
}
