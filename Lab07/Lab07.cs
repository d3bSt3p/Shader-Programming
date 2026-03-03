using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Lab07
{
    public class Lab07 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Shader effect
        Effect effect;

        // lab 3
        Model model;

        // lab 7
        SpriteFont font;

        Texture2D texture;
        
        // Lab 2
        Matrix world;
        Matrix view;
        Matrix projection;


        // lab 3 exersise
        float angle = 0;
        float angle2 = 0;
        float distance = 15.0f;

        float angleL = 0f;
        float angleL2 = 0f;
        Vector3 lightPosition = Vector3.Zero;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private Vector3 cameraPosition;
        private Vector3 cameraTarget;
        
        public Lab07()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Font");
            model = Content.Load<Model>("Plane");
            effect = Content.Load<Effect>("BumpMap");
            texture = Content.Load<Texture2D>("NormalMaps/round");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();


            if (keyboardState.IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (keyboardState.IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (keyboardState.IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (keyboardState.IsKeyDown(Keys.Down)) angleL2 -= 0.02f;

            lightPosition = Vector3.Transform(new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2)
                * Matrix.CreateRotationY(angleL));

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
            if (mouseState.MiddleButton == ButtonState.Pressed) {
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
                Matrix.CreateRotationX(angle2) *Matrix.CreateRotationY(angle) * 
                Matrix.CreateTranslation(cameraTarget));

            view = Matrix.CreateLookAt(cameraPosition, cameraTarget,
                Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle)));

            // clamp distance to avoid going through the model, and flipping to the other side
            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);

            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            effect.CurrentTechnique = effect.Techniques[0];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                   
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["AmbientColor"].SetValue(1.0f);
                        effect.Parameters["AmbientIntensity"].SetValue(0.1f);
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);

                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["normalMap"].SetValue(texture);

                        Matrix worldInverseTranspose =
                            Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

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

            base.Draw(gameTime);
        }
    }
}
