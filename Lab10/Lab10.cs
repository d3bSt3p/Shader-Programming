using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab10
{
    public class Lab10 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Effect effect;

        Texture2D FireTexture;

        Model torusModel;

        // Lab 2 
        Matrix world;
        Matrix view;
        Matrix projection;

        // lab 3
        float angle = 0;
        float angle2 = 100;
        float distance = 20.0f;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private Vector3 cameraPosition;
        private Vector3 cameraTarget;


        //** lab10
        ParticleManager particleManager;
        System.Random random;
        private Vector3 particlePosition;

        public Lab10()
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
            random = new System.Random();
            particleManager = new ParticleManager(GraphicsDevice, 100);
            particlePosition = new Vector3(0, 0, 0);

            torusModel = Content.Load<Model>("Torus");

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

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) *
                Matrix.CreateTranslation(cameraTarget));

            view = Matrix.CreateLookAt(cameraPosition, cameraTarget,
                Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle)));

            // clamp distance to avoid going through the model, and flipping to the other side
            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);
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
                       
            torusModel.Draw(world, view, projection);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState()
            {
                // added info for proper blending of particles
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false
            };
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.CurrentTechnique = effect.Techniques[0];
            Matrix inverseCamera = Matrix.Invert(view);
            inverseCamera.Translation = Vector3.Zero;
            effect.Parameters["InverseCamera"].SetValue(inverseCamera);
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["WorldInverseTranspose"].SetValue(
                Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["Texture"].SetValue(FireTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                particleManager.Draw(GraphicsDevice);
            }

            base.Draw(gameTime);
        }
    }
}
