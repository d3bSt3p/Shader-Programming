using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab3
{
    public class Lab03 : Game
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
        float angle = 0; // for the x axis
        float angle2 = 0; // for the y axis


        float distance = 15.0f;

        Vector3 lightPositon = new Vector3();
        private MouseState previousMouseState;

        public Lab03()
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

            effect = Content.Load<Effect>("Diffuse");
            model = Content.Load<Model>("bunny");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                angle += (Mouse.GetState().X - previousMouseState.X) * 0.01f;
                angle2 += (Mouse.GetState().Y - previousMouseState.Y) * 0.01f;
            }
            if(Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                 distance += (Mouse.GetState().Y - previousMouseState.Y) * 0.1f;
            }

            previousMouseState = Mouse.GetState();

            // clamp angle2 to avoid flipping
            angle2 = MathHelper.Clamp(angle2, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

            // clamp distance to avoid going through the model, and flipping to the other side
            // max distance to avoid going too far is set to 90.0f because thats the furtherst we can zoom out
            // without parts of the model being culled
            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Vector3 cameraDistance = new Vector3(0, 0, distance);
            Vector3 camera = Vector3.Transform(
             cameraDistance,
             Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)
             );
            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            // Camera Projection
            // 
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            //model.Draw(world, view, projection);

            // custom draw
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
                       
                        effect.Parameters["AmbientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                        effect.Parameters["AmbientIntensity"].SetValue(0.2f);
                        effect.Parameters["DiffuseLightDirection"].SetValue(Vector3.Normalize(new Vector3(1, 1, 1)));
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);

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
