using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab09
{

    public class Lab09 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        // **** TEMPLATE ************//
        SpriteFont font;
        Effect effect;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
        Vector3 cameraPosition, cameraTarget, lightPosition;
        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 30;
        MouseState preMouse;
        //Model model;
        Model[] models;
        Texture2D texture;
        // **** TEMPLATE ************//


        // *** Lab08
        Matrix lightView = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
        Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 100f);
        int effectMode = 1;

        public Lab09()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            
            effect = Content.Load<Effect>("ProjectiveTexture");
            texture = Content.Load<Texture2D>("nvlobby_new_negz");
            models = new Model[2];
            models[0] = Content.Load<Model>("Plane");
            models[1] = Content.Load<Model>("torus");
        }

        protected override void UnloadContent()
        {
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.F1)) effectMode = 1;
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) effectMode = 2;

            // ************ TEMPLATE ************ //
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.S)){angle = angle2 = angleL = angleL2 = 0;distance = 30; cameraTarget = Vector3.Zero; }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                angle -= (Mouse.GetState().X - preMouse.X) / 100f;
                angle2 += (Mouse.GetState().Y - preMouse.Y) / 100f;
            }
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().X - preMouse.X) / 100f;
            }

            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                cameraTarget -= ViewRight * (Mouse.GetState().X - preMouse.X) / 10f;
                cameraTarget += ViewUp * (Mouse.GetState().Y - preMouse.Y) / 10f;
            }
            preMouse = Mouse.GetState();
            // Update Camera
            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) * Matrix.CreateTranslation(cameraTarget));
            view = Matrix.CreateLookAt(
                cameraPosition, 
                cameraTarget, 
                Vector3.Transform(Vector3.UnitY,Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));
            // ********************************** //


            // **** Lab 8 (Light Matrix) ****//
            // Update Light
            lightPosition = Vector3.Transform(
                new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));
            // Update LightMatrix
            lightView = Matrix.CreateLookAt(
                lightPosition, Vector3.Zero, 
                Vector3.Transform(Vector3.UnitY,Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL)));
            // ******************************//
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // ************ TEMPLATE ************ //
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            effect.CurrentTechnique = effect.Techniques[0];

            foreach (Model model in models)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        foreach (ModelMeshPart part in mesh.MeshParts)
                        {
                            if (effectMode == 1)
                            {
                                effect = Content.Load<Effect>("ProjectiveTexture");
                                effect.Parameters["AmbientColor"].SetValue(0.5f);
                                effect.Parameters["ProjectiveTexture"].SetValue(texture);
                            }
                            if (effectMode == 2)
                            {
                                effect = Content.Load<Effect>("ShadowMap");
                            }

                            effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                            effect.Parameters["View"].SetValue(view);
                            effect.Parameters["Projection"].SetValue(projection);
                            Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
                            effect.Parameters["LightPosition"].SetValue(lightPosition);
                            effect.Parameters["LightViewMatrix"].SetValue(lightView);
                            effect.Parameters["LightProjectionMatrix"].SetValue(lightProjection);


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
            // ************************************* //
            base.Draw(gameTime);
        }
    }
}
