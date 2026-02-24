using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// import Library
using CPI411.SimpleEngine;

namespace Lab06
{
    public class Lab06 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // lab 1
        Effect effect;

        // lab 3
        Model model;

        // Lab 6
        Texture2D helicopterTexture;


        // Lab 2
        Matrix world;
        Matrix view;
        Matrix projection;


        // lab 3
        float angle = 0;
        float angle2 = 0;
        float distance = 5.0f;

        private MouseState previousMouseState;
        private Vector3 cameraPosition;

        private TextureCube skyboxTexture;


        // Lab 5
        Skybox skybox;
        string[] skyboxImageFiles =
        {
            "skybox/SunsetPNG1",
            "skybox/SunsetPNG2",
            "skybox/SunsetPNG3",
            "skybox/SunsetPNG4",
            "skybox/SunsetPNG5",
            "skybox/SunsetPNG6"
        };
        public Lab06()
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

            effect = Content.Load<Effect>("Reflection");
            model = Content.Load<Model>("helicopter/Helicopter");

            // Lab 6 
            helicopterTexture = Content.Load<Texture2D>("helicopter/HelicopterTexture");
            // Lab 5
            skybox = new Skybox(skyboxImageFiles,Content,_graphics.GraphicsDevice);
            
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
                      

            // Lab 3
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                angle += (currentMouseState.X - previousMouseState.X) * 0.01f;
                angle2 += (currentMouseState.Y - previousMouseState.Y) * 0.01f;
            }
            if (currentMouseState.RightButton == ButtonState.Pressed)
            {
                distance += (currentMouseState.Y - previousMouseState.Y) * 0.1f;
            }

            // Lab 5
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                angle += 0.01f;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                angle -= 0.01f;
            }

            previousMouseState = currentMouseState;

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            Vector3 cameraUp = Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            Vector3 cameraDistance = new Vector3(0, 0, distance);

            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, cameraUp);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            // Clamp the camera camera from looking to far up or down
            //angle2 = MathHelper.Clamp(angle2, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

            // Clamp the distance the camera can zoom in
            //distance = MathHelper.Clamp(distance, 0.1f, 90.0f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Enable depth buffer
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            _graphics.GraphicsDevice.RasterizerState = originalRasterizerState;
            DrawModelWithEffect();

            base.Draw(gameTime);
        }

        private void DrawModelWithEffect()
        {
            effect.CurrentTechnique = effect.Techniques["Reflection"];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["decalMap"].SetValue(helicopterTexture);
                        effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        
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
        }
    }
}
