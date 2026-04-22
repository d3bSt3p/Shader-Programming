using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment04
{
    public class Assignment04 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont font;

        Effect effect;

        // load particle textures
        Texture2D FireTexture;
        Texture2D SmokeTexture;
        Texture2D WaterTexture;

        Model Plane;

        //** lab 2 
        Matrix world;
        Matrix view;
        Matrix projection;

        //** lab 3
        float angle = 0;
        float angle2 = 100;
        float distance = 20.0f;

        //** lab 7
        float angleL = 0f;
        float angleL2 = 0f;
        Vector3 lightPosition = new Vector3(0, 0, 15);

        // Default values camera and light reset
        float defaultAngle = 0;
        float defaultAngle2 = 100;
        float defaultDistance = 20.0f;
        Vector3 defaultCameraTarget = Vector3.Zero;
        float defaultAngleL = 0.8f;
        float defaultAngleL2 = 0.6f;

        // Light properties
        float lightIntensity = 1.0f;
        float specularIntensity = 1.0f;

        // UI display toggles
        bool showHelp = false;
        bool showInfo = false;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private Vector3 cameraPosition;
        private Vector3 cameraTarget;

        //** lab10
        ParticleManager particleManager;
        System.Random random;
        private Vector3 particlePosition;

        private Matrix inverseCamera;

        // Particle mode: 1=Phong(no texture), 2=Smoke, 3=Water, 4=Fire
        private int particleMode = 4;

        private static readonly string[] ParticleModeNames =
        [
            "Phong (No Texture)",
            "Smoke",
            "Water",
            "Fire"
        ];

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

            FireTexture = Content.Load<Texture2D>("fire");
            SmokeTexture = Content.Load<Texture2D>("smoke");
            WaterTexture = Content.Load<Texture2D>("water");

            random = new System.Random();
            particleManager = new ParticleManager(GraphicsDevice, 100);
            particlePosition = new Vector3(0, 0, 0);

            Plane = Content.Load<Model>("Plane");

            world = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f, 1000f);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            #region Light Controlls
            if (keyboardState.IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (keyboardState.IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (keyboardState.IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (keyboardState.IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            #endregion

            #region Camera Controlls
            // Lab 3
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                angle += (mouseState.X - previousMouseState.X) * 0.01f;
                angle2 += (mouseState.Y - previousMouseState.Y) * 0.01f;
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                distance += (mouseState.Y - previousMouseState.Y) * 0.1f;
            }
            // clamp distance to avoid going through the model, and flipping to the other side
            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);

            if (mouseState.MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle));
                cameraTarget -= ViewRight * (mouseState.X - previousMouseState.X) / 10f;
                cameraTarget += ViewUp * (mouseState.Y - previousMouseState.Y) / 10f;
            }
            #endregion

            // Reset camera and light: "S" Key
            if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                angle = defaultAngle;
                angle2 = defaultAngle2;
                distance = defaultDistance;
                cameraTarget = defaultCameraTarget;
                angleL = defaultAngleL;
                angleL2 = defaultAngleL2;
            }

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) *
                Matrix.CreateTranslation(cameraTarget));

            lightPosition = Vector3.Transform(new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));

            view = Matrix.CreateLookAt(cameraPosition, cameraTarget,
                Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle)));

            #region Particle Mode Selection
            if (keyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1)) particleMode = 1;
            if (keyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2)) particleMode = 2;
            if (keyboardState.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3)) particleMode = 3;
            if (keyboardState.IsKeyDown(Keys.D4) && previousKeyboardState.IsKeyUp(Keys.D4)) particleMode = 4;
            #endregion

            #region Particle Generation
            // *** Lab 10
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                Particle particle = particleManager.getNext();
                particle.Position = particlePosition;

                // Task 2: Random velocity for fire-like upward burst with spread
                particle.Velocity = new Vector3(
                    (float)(random.NextDouble() * 2.0 - 1.0),  // random X: -1 to 1
                    (float)(random.NextDouble() * 3.0 + 1.0),  // random Y: 1 to 4 (upward)
                    (float)(random.NextDouble() * 2.0 - 1.0)); // random Z: -1 to 1

                // Task 2: Slight upward acceleration, slight drag on X/Z
                particle.Acceleration = new Vector3(0, 1.0f, 0);

                particle.MaxAge = (float)(random.NextDouble() * 1.5 + 0.5); // 0.5 to 2.0 seconds
                particle.Init();
            }
            particleManager.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            #endregion

            #region Help and Info Toggles
            // Help screen toggle
            if (keyboardState.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
            {
                showHelp = !showHelp;
            }

            // Info display toggle
            if (keyboardState.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
            {
                showInfo = !showInfo;
            }
            #endregion

            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            inverseCamera = Matrix.Invert(view);
            inverseCamera.Translation = Vector3.Zero;

            Plane.Draw(world, view, projection);

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

            // Mode 1: Phong shading, no texture
            // Modes 2-4: Billboard particles with texture
            if (particleMode == 1)
            {
                effect.CurrentTechnique = effect.Techniques["Phong"];
            }
            else
            {
                effect.CurrentTechnique = effect.Techniques["FireParticleTechnique"];

                Texture2D activeTexture = particleMode switch
                {
                    2 => SmokeTexture,
                    3 => WaterTexture,
                    _ => FireTexture   // mode 4
                };
                effect.Parameters["Texture"].SetValue(activeTexture);
            }

            effect.Parameters["LightPosition"].SetValue(lightPosition);
            effect.Parameters["InverseCamera"].SetValue(inverseCamera);
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["WorldInverseTranspose"].SetValue(
                Matrix.Transpose(Matrix.Invert(world)));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                particleManager.Draw(GraphicsDevice);
            }
        }

        private void DrawUIStateManagement()
        {
            // Draw UI text with proper state management
            if (font != null && (showHelp || showInfo))
            {
                // properly overlay text
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null);

                if (showHelp)
                {
                    DrawHelpScreen();
                }

                if (showInfo)
                {
                    DrawInfoDisplay();
                }

                _spriteBatch.End();

                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            }
        }

        private void DrawHelpScreen()
        {
            string helpText =
                "=== CONTROLS ===\n\n" +
                "CAMERA:\n" +
                "  Left Mouse Drag: Rotate camera\n" +
                "  Right Mouse Drag: Zoom in/out\n" +
                "  Middle Mouse Drag: Pan camera\n" +
                "  S: Reset camera and light\n\n" +
                "LIGHT:\n" +
                "  Arrow Keys: Rotate light\n\n" +
                "PARTICLE MODE:\n" +
                "  1: Phong shading (no texture)\n" +
                "  2: Smoke texture\n" +
                "  3: Water texture\n" +
                "  4: Fire texture\n" +
                "  P: Emit particles\n\n" +
                "UI:\n" +
                "  ?: Toggle this help screen\n" +
                "  H: Toggle info display";

            _spriteBatch.DrawString(font, helpText, new Vector2(10, 10), Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }

        private void DrawInfoDisplay()
        {
            string infoText =
                "=== SHADER INFO ===\n\n" +
                $"Particle Mode: {particleMode} - {ParticleModeNames[particleMode - 1]}\n" +
                $"Technique: {effect.CurrentTechnique.Name}\n\n" +
                $"Camera Angle: ({angle:F2}, {angle2:F2})\n" +
                $"Camera Distance: {distance:F2}\n\n" +
                $"Light Angle: ({angleL:F2}, {angleL2:F2})\n" +
                $"Light Intensity: {lightIntensity:F2}\n\n";

            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width - 350, 10);
            _spriteBatch.DrawString(font, infoText, position, Color.Yellow, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }
    }
}
