using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Lab4
{
    public class Lab04 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        // Shader effect
        Effect effect;

        // lab 3
        Model model;

        // Lab 2
        Matrix world;
        Matrix view;
        Matrix projection;


        // lab 3 exersise
        float angle = 0; 
        float angle2 = 0; 
        float distance = 15.0f;
       
        private MouseState previousMouseState;
        private Vector3 cameraPosition;

        // Lab 4 - Shininess control
        float shininess = 20.0f;

        // Lab 4 - Technique selection
        int currentTechnique = 0; 
        private KeyboardState previousKeyboardState;

        public Lab04()
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

            effect = Content.Load<Effect>("SimpleShaders");
            model = Content.Load<Model>("Torus");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Lab 3
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                angle += (Mouse.GetState().X - previousMouseState.X) * 0.01f;
                angle2 += (Mouse.GetState().Y - previousMouseState.Y) * 0.01f;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().Y - previousMouseState.Y) * 0.1f;
            }

            previousMouseState = Mouse.GetState();

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            Vector3 cameraUp = Vector3.Transform(Vector3.UnitY,Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, cameraUp);

            // clamp distance to avoid going through the model, and flipping to the other side
            // max distance to avoid going too far is set to 90.0f because thats the furtherst we can zoom out
            // without parts of the model being culled
            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);


            // Lab 4
            // Shininess controls
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                shininess += 1.0f;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                shininess -= 1.0f;
            }

            // Clamp shininess to reasonable values
            shininess = MathHelper.Clamp(shininess, 1.0f, 200.0f);

            // Technique switching with number keys
            if (keyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
            {
                effect.CurrentTechnique = effect.Techniques["Gouraud"];
            }
            if (keyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
            {
                effect.CurrentTechnique = effect.Techniques["Phong"];
            }
            if (keyboardState.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
            {
                effect.CurrentTechnique = effect.Techniques["Toon"];
            }

            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["AmbientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                        effect.Parameters["AmbientIntensity"].SetValue(0.2f);
                        effect.Parameters["DiffuseLightDirection"].SetValue(Vector3.Normalize(new Vector3(10, 10, 10)));
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);

                        // Lab 4 specular parameters
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["SpecularIntensity"].SetValue(1.0f);
                        effect.Parameters["Shininess"].SetValue(shininess);

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
