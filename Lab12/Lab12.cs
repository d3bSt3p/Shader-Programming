using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab12
{
    public class Lab12 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        // **** TEMPLATE ************//
        SpriteFont font;
        Effect effect;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 0), new Vector3(0, 0, 30), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
        Vector3 cameraPosition, cameraTarget, lightPosition;
        float angle = 4;
        float angle2 = -0.8f;
        float distance = 20;
        MouseState preMouse;

        // **** TEMPLATE ************//

        // *** Lab12
        Model[] models;
        RenderTarget2D renderTarget;
        Texture2D randomNormalMap, depthAndNormalMap;
        float offset = 800f / 256f;
        float SSAORad = 0.0001f;

        VertexPositionTexture[] vertices =
            {
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1))};

        // *** Debug UI
        bool showHelp = false;
        bool showInfo = false;
        KeyboardState previousKeyboardState;

        public Lab12()
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
            models = new Model[2];
            models[0] = Content.Load<Model>("Plane");
            models[1] = Content.Load<Model>("objects");

            randomNormalMap = Content.Load<Texture2D>("noise");

            // *** Lab9: Step1 - Create the render target
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = 
                new RenderTarget2D(GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight,
                false, SurfaceFormat.Color, DepthFormat.Depth24);
                     
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // ************ TEMPLATE ************ //           
            if (keyboardState.IsKeyDown(Keys.S)) { angle = 4; angle2 = -0.8f; distance = 20; cameraTarget = Vector3.Zero; }
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                angle -= (mouseState.X - preMouse.X) / 100f;
                angle2 += (mouseState.Y - preMouse.Y) / 100f;
            }
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                distance -= (mouseState.Y - preMouse.Y) * 0.1f;
            }

                if (mouseState.MiddleButton == ButtonState.Pressed)
                {
                    Vector3 ViewRight = Vector3.Transform(Vector3.UnitX,
                        Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                    Vector3 ViewUp = Vector3.Transform(Vector3.UnitY,
                        Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                    cameraTarget -= ViewRight * (mouseState.X - preMouse.X) / 10f;
                    cameraTarget += ViewUp * (mouseState.Y - preMouse.Y) / 10f;
                }
                preMouse = mouseState;
                // Update Camera
                cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) * Matrix.CreateTranslation(cameraTarget));
                view = Matrix.CreateLookAt(
                    cameraPosition,
                    cameraTarget,
                    Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));
                // ********************************** //           

                // Debug UI toggles
                if (keyboardState.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
                    showHelp = !showHelp;
                if (keyboardState.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
                    showInfo = !showInfo;

                // *** Lab 12 SSAO parameter adjustments
                if (keyboardState.IsKeyDown(Keys.OemPlus)) SSAORad *= 1.05f;
                if (keyboardState.IsKeyDown(Keys.OemMinus)) SSAORad *= 0.95f;

                previousKeyboardState = keyboardState;

                base.Update(gameTime);
            }
     

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawDepthAndNormalMap();
            GraphicsDevice.SetRenderTarget(null);
            depthAndNormalMap = (Texture2D)renderTarget;

            //*** This block will be used later for Deferred Shading (SSAO)
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            DrawSSAO();
            
            /*using (SpriteBatch sprite = new SpriteBatch(GraphicsDevice))
            {
                sprite.Begin();
                sprite.Draw(depthAndNormalMap, new Vector2(0, 0), null,
                 Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                sprite.End();
            }*/

            DrawDebugUI();
         
            base.Draw(gameTime);
        }

        private void DrawDepthAndNormalMap()
        {
            effect = Content.Load<Effect>("DepthAndNormal");
            effect.CurrentTechnique = effect.Techniques[0];
            foreach (Model model in models)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        foreach (ModelMeshPart part in mesh.MeshParts)
                        {
                            effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                            effect.Parameters["View"].SetValue(view);
                            effect.Parameters["Projection"].SetValue(projection);
                            Matrix worldInverseTransposeMatrix =
                             Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
                            pass.Apply();
                            GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                            GraphicsDevice.Indices = part.IndexBuffer;
                            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                            part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                        }
                    }
                }
            }          
        }


        private void DrawSSAO()
        {
            effect = Content.Load<Effect>("SSAO");
            effect.CurrentTechnique = effect.Techniques[0];
            effect.CurrentTechnique.Passes[0].Apply();
            effect.Parameters["RandomNormalTexture"].SetValue(randomNormalMap);
            effect.Parameters["DepthAndNormalTexture"].SetValue(depthAndNormalMap);
            effect.Parameters["offset"].SetValue(offset);
            effect.Parameters["rad"].SetValue(SSAORad);
            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
            (PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
        }

        private void DrawDebugUI()
        {
            if (font == null || (!showHelp && !showInfo)) return;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null);

            if (showHelp)
                DrawHelpScreen();

            if (showInfo)
                DrawInfoDisplay();

            spriteBatch.End();

            // Restore 3D render states after SpriteBatch
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        private void DrawHelpScreen()
        {
            string helpText =
                "=== CONTROLS ===\n\n" +
                "CAMERA:\n" +
                "  Left Mouse Drag: Rotate camera\n" +
                "  Middle Mouse Drag: Pan camera\n" +
                "  S: Reset all\n\n" +
                "UI:\n" +
                "  ?: Toggle this help screen\n" +
                "  H: Toggle info display" +
                "SSAORad:\n"+
                "  +: Increase SSAO radius\n" +
                "  -: Decrease SSAO radius";
                              

            spriteBatch.DrawString(font, helpText, new Vector2(10, 10), Color.Black, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }

        private void DrawInfoDisplay()
        {
            string infoText = "=== LAB 12 INFO ===\n\n" +
                              $"Camera Angle:    ({angle:F2}, {angle2:F2})\n" +
                              $"Camera Distance: {distance:F2}\n" +
                              $"Camera Position: ({cameraPosition.X:F2}, {cameraPosition.Y:F2}, {cameraPosition.Z:F2})\n" +
                              $"Camera Target:   ({cameraTarget.X:F2}, {cameraTarget.Y:F2}, {cameraTarget.Z:F2})\n" +
                              $"SSAO Radius:    {SSAORad:F2}\n\n";
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width - 380, 10);
            spriteBatch.DrawString(font, infoText, position, Color.Black, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }
    }
}
