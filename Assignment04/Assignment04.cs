using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Assignment04
{
    public class Assignment04 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont font;
        Effect effect;

        // Particle textures
        Texture2D FireTexture;
        Texture2D SmokeTexture;
        Texture2D WaterTexture;

        Model Plane;

        // Lab 2 
        Matrix world;
        Matrix view;
        Matrix projection;

        // lab 3
        float angle = 0;
        float angle2 = 100;
        float distance = 20.0f;

        // Light orbit state
        float angleL = 0f;
        float angleL2 = 0f;
        Vector3 lightPosition = new Vector3(0, 0, 15);

        // Default reset values
        const float DefaultAngle = 0f;
        const float DefaultAngle2 = 100f;
        const float DefaultDistance = 20.0f;
        const float DefaultAngleL = 0.8f;
        const float DefaultAngleL2 = 0.6f;
        Vector3 defaultCameraTarget = Vector3.Zero;

        // Light / specular
        float specularIntensity = 0.5f;

        // UI toggles
        bool showHelp = false;
        bool showInfo = false;
        bool isEmitting = false;

        // Lab 4
        KeyboardState previousKeyboardState;
        MouseState previousMouseState;
        Vector3 cameraPosition;
        Vector3 cameraTarget;

        //** lab10
        ParticleManager particleManager;
        Random random;
        Vector3 particlePosition;
        Matrix  inverseCamera;
             
        private int particleMode = 4;

        private static readonly string[] ParticleModeNames =
        [
            "Colored quads (no texture)",
            "Smoke",
            "Water",
            "Fire"
        ];
           
        private int fountainMode = 1;

        private static readonly string[] FountainModeNames =
        [
            "F1:Basic",
            "F2:Medium (gravity)",
            "F3:Advanced (bounce + wind)"
        ];
                
        private static readonly Vector3 Gravity = new Vector3(0f, -9.8f, 0f);

        private bool  fountainUp = true;   
        private float velocityMult = 3.0f;   
        private float particleLifespan = 2.0f;   
        private float particleSize = 1.0f;   
        private float particleResilience = 0.6f;  
        private float particleFriction = 0.8f;  
        private float emitterRadius = 2.0f;  
        private float windStrength = 0.0f;  
        private float windAngle = 0.0f;  
                
        private Vector3 windVector = Vector3.Zero;
                
        private const float StepVelocity = 0.5f;
        private const float StepSize = 0.1f;
        private const float StepLifespan = 0.25f;
        private const float StepResilience = 0.05f;
        private const float StepFriction = 0.05f;
        private const float StepEmitter = 0.25f;
        private const float StepWind = 0.5f;
        private const float StepWindAngle = 0.1f;

        public Assignment04()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("ParticleShader");
            font = Content.Load<SpriteFont>("Font");

            FireTexture = Content.Load<Texture2D>("fire");
            SmokeTexture = Content.Load<Texture2D>("smoke");
            WaterTexture = Content.Load<Texture2D>("water");

            random = new Random();
            particleManager = new ParticleManager(GraphicsDevice, 500);
            particlePosition = Vector3.Zero;

            Plane = Content.Load<Model>("Plane");

            world = Matrix.Identity;

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f, 1000f);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState KeyboardState = Keyboard.GetState();
            MouseState MouseState = Mouse.GetState();
            float DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || KeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            #region Camera controls
            if (MouseState.LeftButton == ButtonState.Pressed)
            {
                angle += (MouseState.X - previousMouseState.X) * 0.01f;
                angle2 += (MouseState.Y - previousMouseState.Y) * 0.01f;
            }
            if (MouseState.RightButton == ButtonState.Pressed)
                distance += (MouseState.Y - previousMouseState.Y) * 0.1f;

            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);

            if (MouseState.MiddleButton == ButtonState.Pressed)
            {
                Matrix rot = Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle);
                Vector3 right = Vector3.Transform(Vector3.UnitX, rot);
                Vector3 up = Vector3.Transform(Vector3.UnitY, rot);
                cameraTarget -= right * (MouseState.X - previousMouseState.X) / 10f;
                cameraTarget += up * (MouseState.Y - previousMouseState.Y) / 10f;
            }
            #endregion

            #region Light controls
            if (KeyboardState.IsKeyDown(Keys.Left)) angleL  += 0.02f;
            if (KeyboardState.IsKeyDown(Keys.Right)) angleL  -= 0.02f;
            if (KeyboardState.IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (KeyboardState.IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            #endregion

            #region Reset camera + light
            if (KeyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                angle = DefaultAngle;
                angle2 = DefaultAngle2;
                distance = DefaultDistance;
                cameraTarget = defaultCameraTarget;
                angleL = DefaultAngleL;
                angleL2 = DefaultAngleL2;
            }
            #endregion
                        
            Matrix camRot = Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle);
            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                camRot * Matrix.CreateTranslation(cameraTarget));
            lightPosition  = Vector3.Transform(new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));
            view = Matrix.CreateLookAt(cameraPosition, cameraTarget,
                Vector3.Transform(Vector3.UnitY, camRot));

            #region Render mode (1-4)
            if (KeyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1)) particleMode = 1;
            if (KeyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2)) particleMode = 2;
            if (KeyboardState.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3)) particleMode = 3;
            if (KeyboardState.IsKeyDown(Keys.D4) && previousKeyboardState.IsKeyUp(Keys.D4)) particleMode = 4;
            #endregion

            #region Fountain mode (F1-F3 function keys)
            if (KeyboardState.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1)) fountainMode = 1;
            if (KeyboardState.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2)) fountainMode = 2;
            if (KeyboardState.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3)) fountainMode = 3;
            #endregion

            #region Parameter tuning
            // Velocity multiplier: V (Decrease) / B (Increase)
            if (KeyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
                velocityMult = Math.Max(0.5f, velocityMult + StepVelocity);
            if (KeyboardState.IsKeyDown(Keys.V) && previousKeyboardState.IsKeyUp(Keys.V))
                velocityMult = Math.Max(0.5f, velocityMult - StepVelocity);

            // Particle size: - (Decrease) / + (Increase)
            if (KeyboardState.IsKeyDown(Keys.OemPlus) && previousKeyboardState.IsKeyUp(Keys.OemPlus))
                particleSize = Math.Max(0.1f, particleSize + StepSize);
            if (KeyboardState.IsKeyDown(Keys.OemMinus) && previousKeyboardState.IsKeyUp(Keys.OemMinus))
                particleSize = Math.Max(0.1f, particleSize - StepSize);

            // Lifespan: L (decrease) / K (increase)
            if (KeyboardState.IsKeyDown(Keys.K) && previousKeyboardState.IsKeyUp(Keys.K))
                particleLifespan = Math.Max(0.25f, particleLifespan - StepLifespan);
            if (KeyboardState.IsKeyDown(Keys.L) && previousKeyboardState.IsKeyUp(Keys.L))
                particleLifespan = Math.Max(0.25f, particleLifespan + StepLifespan);

            // Resilience (F3): T (decrease) / R (increase)
            if (KeyboardState.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T))
                particleResilience = MathHelper.Clamp(particleResilience + StepResilience, 0f, 1f);
            if (KeyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
                particleResilience = MathHelper.Clamp(particleResilience - StepResilience, 0f, 1f);

            // Friction (F3):  G (decrease) / F (increase)
            if (KeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G))
                particleFriction = MathHelper.Clamp(particleFriction + StepFriction, 0f, 1f);
            if (KeyboardState.IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))
                particleFriction = MathHelper.Clamp(particleFriction - StepFriction, 0f, 1f);

            // Emitter radius:  M (decrease) / N (increase)
            if (KeyboardState.IsKeyDown(Keys.M) && previousKeyboardState.IsKeyUp(Keys.N))
                emitterRadius = Math.Max(0.25f, emitterRadius + StepEmitter);
            if (KeyboardState.IsKeyDown(Keys.N) && previousKeyboardState.IsKeyUp(Keys.M))
                emitterRadius = Math.Max(0.25f, emitterRadius - StepEmitter);

            // Wind strength (F3): W (increase) / Q (decrease)
            if (KeyboardState.IsKeyDown(Keys.W) && previousKeyboardState.IsKeyUp(Keys.W))
                windStrength = Math.Max(0f, windStrength + StepWind);
            if (KeyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
                windStrength = Math.Max(0f, windStrength - StepWind);

            // Wind direction: A (counterclockwise) / D (clockwise)
            if (KeyboardState.IsKeyDown(Keys.A)) windAngle += StepWindAngle * DeltaTime  * 60f;
            if (KeyboardState.IsKeyDown(Keys.D)) windAngle -= StepWindAngle * DeltaTime * 60f;
            #endregion
                        
            windVector = new Vector3(
                (float)Math.Cos(windAngle) * windStrength,
                0f,
                (float)Math.Sin(windAngle) * windStrength);

            #region Gravity + wind applied to active particles each frame
            foreach (Particle p in particleManager.particles)
            {
                if (!p.IsActive() || !p.UseGravity) continue;
                p.Acceleration = Gravity + windVector;
            }
            #endregion

            #region Particle emission 
            bool shiftHeld = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);

            if (shiftHeld && KeyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
                isEmitting = !isEmitting;
            else if (!shiftHeld && KeyboardState.IsKeyDown(Keys.P))
                EmitParticle();

            if (isEmitting)
                EmitParticle();
            #endregion

            particleManager.Update(DeltaTime);

            #region UI toggles
            if (KeyboardState.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
                showHelp = !showHelp;
            if (KeyboardState.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
                showInfo = !showInfo;
            #endregion

            previousKeyboardState = KeyboardState;
            previousMouseState    = MouseState;
            base.Update(gameTime);
        }

        private Vector3 GetEmitPosition()
        {
            return SquarePosition();
        }

        private Vector3 SquarePosition()
        {
            float x = (float)(random.NextDouble() * 2.0 - 1.0) * emitterRadius;
            float z = (float)(random.NextDouble() * 2.0 - 1.0) * emitterRadius;
            return particlePosition + new Vector3(x, 0, z);
        }

        private void EmitParticle()
        {
            Particle particle = particleManager.getNext();
            particle.Position = GetEmitPosition();
            particle.Size = particleSize;
            particle.MaxAge = particleLifespan + (float)(random.NextDouble() * 0.4 - 0.2);

            switch (fountainMode)
            {
                case 1: // F1: straight line, constant velocity
                    particle.Velocity = new Vector3(0f, (fountainUp ? 1f : -1f) * velocityMult * 3f, 0f);
                    particle.Acceleration = Vector3.Zero;
                    particle.UseGravity = false;
                    particle.MaxBounces = 0;
                    break;

                case 2: // F2: random directions + gravity
                    particle.Velocity = RandomUpwardVelocity();
                    particle.Acceleration = Gravity;
                    particle.UseGravity = true;
                    particle.MaxBounces = 0;
                    break;

                case 3: // F3: gravity + bounce + wind
                    particle.Velocity = RandomUpwardVelocity();
                    particle.Acceleration = Gravity + windVector;
                    particle.UseGravity = true;
                    particle.Resilience = particleResilience;
                    particle.Friction = particleFriction;
                    particle.MaxBounces = 3;
                    break;
            }

            particle.Init();
            particle.Size = particleSize;
        }

        private Vector3 RandomUpwardVelocity()
        {
            float speed = velocityMult;

            // Task 2: Random velocity for fire-like upward burst with spread
            return new Vector3(
                (float)(random.NextDouble() * 2.0 - 1.0) * speed,
                (float)(random.NextDouble() * 2.0 + 0.5) * speed,
                (float)(random.NextDouble() * 2.0 - 1.0) * speed);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            inverseCamera = Matrix.Invert(view);
            inverseCamera.Translation = Vector3.Zero;

            DrawPlane();
            DrawParticles();
            DrawUIStateManagement();

            base.Draw(gameTime);
        }

        private void DrawParticles()
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false
            };
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (particleMode == 1)
            {
                effect.CurrentTechnique = effect.Techniques["PhongParticleTechnique"];
                effect.Parameters["PhongParticleColor"].SetValue(new Vector4(1.0f, 0.3f, 0.1f, 1.0f));
                effect.Parameters["AmbientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1f));
                effect.Parameters["AmbientIntensity"].SetValue(0.5f);
                effect.Parameters["DiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1f));
                effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
                effect.Parameters["SpecularColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
                effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                effect.Parameters["Shininess"].SetValue(32f);
            }
            else
            {
                effect.CurrentTechnique = effect.Techniques["FireParticleTechnique"];
                Texture2D tex = particleMode switch
                {
                    2 => SmokeTexture,
                    3 => WaterTexture,
                    4 => FireTexture,
                };
                effect.Parameters["Texture"].SetValue(tex);
            }

            effect.Parameters["LightPosition"].SetValue(lightPosition);
            effect.Parameters["InverseCamera"].SetValue(inverseCamera);
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                particleManager.Draw(GraphicsDevice);
            }
        }

        private void DrawPlane()
        {
            effect.CurrentTechnique = effect.Techniques["Phong"];
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["LightPosition"].SetValue(lightPosition);
            effect.Parameters["AmbientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1f));
            effect.Parameters["AmbientIntensity"].SetValue(0.5f);
            effect.Parameters["DiffuseColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 1f));
            effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
            effect.Parameters["SpecularColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
            effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
            effect.Parameters["Shininess"].SetValue(32f);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in Plane.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        Matrix meshWord = mesh.ParentBone.Transform;
                        effect.Parameters["World"].SetValue(meshWord);
                        effect.Parameters["WorldInverseTranspose"].SetValue(
                            Matrix.Transpose(Matrix.Invert(meshWord)));
                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount);
                    }
                }
            }
        }

        private void DrawUIStateManagement()
        {
            if (font == null || (!showHelp && !showInfo)) return;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (showHelp) DrawHelpScreen();
            if (showInfo) DrawInfoDisplay();

            _spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        private void DrawHelpScreen()
        {
            string helpText =
                "=== CONTROLS ===\n\n" +
                "CAMERA:\n" +
                "Left Mouse Drag: Rotate\n" +
                "Right Mouse Drag: Zoom\n" +
                "Middle Mouse Drag: Pan\n" +
                "S: Reset camera & light\n\n" +
                "LIGHT:  Arrow Keys\n\n" +
                "RENDER MODE (1-4): Phong / Smoke / Water / Fire\n\n" +
                "FOUNTAIN MODE:\n" +
                "F1: Basic - straight line\n" +
                "F2: Medium - gravity\n" +
                "F3: Advanced - bounce + wind\n" +
                "EMIT:  Hold P (single burst)  |  Shift+P (toggle auto-emit)\n\n" +
                "PARAMETERS:\n" +
                "V / B: Velocity\n" +
                "- / +: Particle size\n" +
                "L / K: Lifespan\n" +
                "R / T: Resilience (F3)\n" +
                "F / G: Friction (F3)\n" +
                "N / M: Emitter radius \n" +
                "W / Q: Wind strength (F3)\n" +
                "A / D: Wind direction\n\n" +
                "UI:  ? toggle help   H toggle info";

            _spriteBatch.DrawString(font, helpText, new Vector2(10, 10),
                Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }

        private void DrawInfoDisplay()
        {
            string infoText =
                "=== PARTICLE INFO ===\n\n" +
                $"Fountain Mode: {FountainModeNames[fountainMode - 1]}\n" +
                $"Render Mode: {particleMode} - {ParticleModeNames[particleMode - 1]}\n\n" +
                $"Velocity Mult: {velocityMult:F2}\n" +
                $"Particle Size: {particleSize:F2}\n" +
                $"Lifespan: {particleLifespan:F2} s\n" +
                $"Emitter Radius: {emitterRadius:F2}\n\n" +
                $"Resilience: {particleResilience:F2}\n" +
                $"Friction: {particleFriction:F2}\n\n" +
                $"Wind Strength: {windStrength:F2}\n" +
                $"Wind Direction: {MathHelper.ToDegrees(windAngle):F1} deg\n\n" +
                $"F1 Direction: {(fountainUp ? "Up" : "Down")}\n";

            Vector2 pos = new Vector2(GraphicsDevice.Viewport.Width - 360, 10);
            _spriteBatch.DrawString(font, infoText, pos,
                Color.Yellow, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }
    }
}
