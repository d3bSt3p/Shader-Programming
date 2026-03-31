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
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 0), new Vector3(0, 0, 30), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
        Vector3 cameraPosition, cameraTarget, lightPosition;
        float angle = 4;
        float angle2 = -0.8f;
        float angleL = 0;
        float angleL2 = -1;
        float distance = 30;
        MouseState preMouse;
        //Model model;
        Model[] models;
        Texture2D texture;
        // **** TEMPLATE ************//

        // *** Lab08
        Matrix lightView = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);
        Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 100f);

        // *** Lab09
        RenderTarget2D renderTarget;
        Texture2D shadowMap;

        // *** Debug UI
        bool showHelp = false;
        bool showInfo = false;
        KeyboardState previousKeyboardState;

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

            effect = Content.Load<Effect>("ShadowMap");
            texture = Content.Load<Texture2D>("nvlobby_new_negz");
            models = new Model[2];
            models[0] = Content.Load<Model>("Plane");
            models[1] = Content.Load<Model>("torus2");

            // *** Lab9: Step1 - Create the render target
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, 2048, 2048, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0,
                RenderTargetUsage.PlatformContents);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // ************ TEMPLATE ************ //
            if (keyboardState.IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (keyboardState.IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (keyboardState.IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (keyboardState.IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            if (keyboardState.IsKeyDown(Keys.S)) { angle = 4; angle2 = -0.8f; angleL2 = -1;  angleL =  0; distance = 30; cameraTarget = Vector3.Zero; }
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
                Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));
            // ********************************** //


            // **** Lab 8 (Light Matrix) ****//
            // Update Light
            lightPosition = Vector3.Transform(
                new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));
            // Update LightMatrix
            lightView = Matrix.CreateLookAt(
                lightPosition, Vector3.Zero,
                Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL)));
            // ******************************//

            // Debug UI toggles
            if (keyboardState.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
                showHelp = !showHelp;
            if (keyboardState.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
                showInfo = !showInfo;

            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            // *** Lab 9: Step2 - Render shadow map to render target
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                Color.Black, 1.0f, 0);
            DrawShadowMap();

            // *** Lab 9: Step4 - Capture shadow map and restore back buffer
            GraphicsDevice.SetRenderTarget(null);
            shadowMap = (Texture2D)renderTarget;

            // *** Lab 9: Step5 - Render the shadowed scene
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                Color.DarkSlateBlue, 1.0f, 0);
            DrawShadowedScene();

            // *** Lab 9: Step7 - Clear the shadow map
            shadowMap = null;

            // *** Debug UI
            DrawDebugUI();

            base.Draw(gameTime);
        }

        private void DrawShadowMap()
        {
            effect.CurrentTechnique = effect.Techniques["ShadowMap"];
            foreach (Model model in models)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        foreach (ModelMeshPart part in mesh.MeshParts)
                        {
                            effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
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
        }

        private void DrawShadowedScene()
        {
            effect.CurrentTechnique = effect.Techniques["ShadowedScene"];
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
                            effect.Parameters["WorldInverseTranspose"].SetValue(
                                Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform)));
                            effect.Parameters["LightPosition"].SetValue(lightPosition);
                            effect.Parameters["LightViewMatrix"].SetValue(lightView);
                            effect.Parameters["LightProjectionMatrix"].SetValue(lightProjection);
                            effect.Parameters["ProjectiveTexture"].SetValue(shadowMap);

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
            string helpText = "=== CONTROLS ===\n\n" +
                              "CAMERA:\n" +
                              "  Left Mouse Drag: Rotate camera\n" +
                              "  Middle Mouse Drag: Pan camera\n" +
                              "  S: Reset all\n\n" +
                              "LIGHT:\n" +
                              "  Arrow Keys: Rotate light\n\n" +
                              "UI:\n" +
                              "  ?: Toggle this help screen\n" +
                              "  H: Toggle info display";

            spriteBatch.DrawString(font, helpText, new Vector2(10, 10), Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }

        private void DrawInfoDisplay()
        {
            string infoText = "=== LAB 09 INFO ===\n\n" +
                              $"Camera Angle:    ({angle:F2}, {angle2:F2})\n" +
                              $"Camera Distance: {distance:F2}\n" +
                              $"Camera Position: ({cameraPosition.X:F2}, {cameraPosition.Y:F2}, {cameraPosition.Z:F2})\n" +
                              $"Camera Target:   ({cameraTarget.X:F2}, {cameraTarget.Y:F2}, {cameraTarget.Z:F2})\n\n" +
                              $"Light Angle:     ({angleL:F2}, {angleL2:F2})\n" +
                              $"Light Position:  ({lightPosition.X:F2}, {lightPosition.Y:F2}, {lightPosition.Z:F2})\n\n";

            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width - 380, 10);
            spriteBatch.DrawString(font, infoText, position, Color.Yellow, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }
    }
}
