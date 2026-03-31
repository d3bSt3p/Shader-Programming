// import Library
using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment03
{
    public class Assignment03 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Shader effect
        Effect effect;

        // Lab 3
        Model torusModel;

        // assignment 3 textures
        Texture2D ArtTexture;
        Texture2D BumpTestTexture;
        Texture2D CrossHatchTexture;
        Texture2D MonkeyTexture;
        Texture2D nmTexture;
        Texture2D RoundTexture;
        Texture2D SaintTexture;
        Texture2D ScienceTexture;
        Texture2D SquareTexture;

        Texture2D CurrentTexture;

        // Lab 2 
        Matrix world;
        Matrix view;
        Matrix projection;

        // lab 3
        float angle = 0;
        float angle2 = 100;
        float distance = 20.0f;

        private MouseState previousMouseState;

        private Vector3 cameraPosition;

        // Lab 4
        float shininess = 20.0f;

        // Lab 4
        int currentTechnique = 0;
        private KeyboardState previousKeyboardState;

        // Assignment 1
        // Camera translation
        private Vector3 cameraTarget;

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

        // Refraction parameters
        float refractionIndex = 0.66f;

        // Refraction dispersion parameters (eta ratios for RGB)
        float etaRatioRed = 0.95f;
        float etaRatioGreen = 1.0f;
        float etaRatioBlue = 1.05f;

        // Fresnel parameters
        float fresnelBias = 0.1f;
        float fresnelScale = 0.9f;
        float fresnelPower = 5.0f;

        // Bump tiling (UV scale) and height
        float bumpTilingU = 1.0f;
        float bumpTilingV = 1.0f;
        float bumpHeight = 1.0f;

        // UI display toggles
        bool showHelp = false;
        bool showInfo = false;

        // Mip-map toggle (M key) — default ON to show good quality
        bool useMipMap = true;

        // Font for rendering text
        SpriteFont font;

        // Lab 5 - Skybox texture sets
        Skybox skybox;

        string[] skyboxOffice =
        {
            "skybox/nvlobby_new_negx", // left  
            "skybox/nvlobby_new_posx", // right
            "skybox/nvlobby_new_posy", // top
            "skybox/nvlobby_new_negy", // bottom 
            "skybox/nvlobby_new_negz", // front 
            "skybox/nvlobby_new_posz"  // back
        };

        public Assignment03()
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

            // Load shader effect
            effect = Content.Load<Effect>("BumpMap");

            // Load all models
            torusModel = Content.Load<Model>("3dModels/Torus");

            // Load all textures
            ArtTexture = Content.Load<Texture2D>("normalMaps/art");
            BumpTestTexture = Content.Load<Texture2D>("normalMaps/BumpTest");
            CrossHatchTexture = Content.Load<Texture2D>("normalMaps/crossHatch");
            MonkeyTexture = Content.Load<Texture2D>("normalMaps/monkey");
            //nmTexture is a cube map? so I am confused why we need to load it?
            //nmTexture = Content.Load<Texture>("normalMaps/nm");
            RoundTexture = Content.Load<Texture2D>("normalMaps/round");
            SaintTexture = Content.Load<Texture2D>("normalMaps/saint");
            ScienceTexture = Content.Load<Texture2D>("normalMaps/science");
            SquareTexture = Content.Load<Texture2D>("normalMaps/square");

            CurrentTexture = ArtTexture;

            // Lab 5 - Initialize with default skybox
            skybox = new Skybox(skyboxOffice, Content, _graphics.GraphicsDevice);

            // Load font for UI
            font = Content.Load<SpriteFont>("Font");

            // Set default technique
            effect.CurrentTechnique = effect.Techniques["BumpMapping"];
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            bool shiftHeld = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

            #region Camera and Light Controls

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

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

            view = Matrix.CreateLookAt(cameraPosition, cameraTarget,
                Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle)));

            // clamp distance to avoid going through the model, and flipping to the other side
            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);

            #endregion

            #region Bump Parameter Controls

            // Bump tiling U: U key (Shift to decrease)
            if (keyboardState.IsKeyDown(Keys.U) )
                bumpTilingU = MathHelper.Clamp(bumpTilingU + (shiftHeld ? -0.05f : 0.05f), 0.1f, 20.0f);

            // Bump tiling V: V key (Shift to decrease)
            if (keyboardState.IsKeyDown(Keys.V))
                bumpTilingV = MathHelper.Clamp(bumpTilingV + (shiftHeld ? -0.05f : 0.05f), 0.1f, 20.0f);

            // Bump height: W key (Shift to decrease)
            if (keyboardState.IsKeyDown(Keys.W))
                bumpHeight = MathHelper.Clamp(bumpHeight + (shiftHeld ? -0.05f : 0.05f), 0.0f, 5.0f);

            // Mip-map toggle: M key
            if (keyboardState.IsKeyDown(Keys.M) && previousKeyboardState.IsKeyUp(Keys.M))
            {
                useMipMap = !useMipMap;
            }

            #endregion
                      
            #region Texture Switching
            // texture switching with number keys
            if (keyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
            {
                CurrentTexture = ArtTexture;
            }
            if (keyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
            {
                CurrentTexture = BumpTestTexture;
            }
            if (keyboardState.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
            {
                CurrentTexture = CrossHatchTexture;
            }
            if (keyboardState.IsKeyDown(Keys.D4) && previousKeyboardState.IsKeyUp(Keys.D4))
            {
                CurrentTexture = MonkeyTexture;
            }

            if (keyboardState.IsKeyDown(Keys.D5) && previousKeyboardState.IsKeyUp(Keys.D5))
            {
                CurrentTexture = nmTexture;
            }

            if (keyboardState.IsKeyDown(Keys.D6) && previousKeyboardState.IsKeyUp(Keys.D6))
            {
                CurrentTexture = RoundTexture;
            }

            if (keyboardState.IsKeyDown(Keys.D7) && previousKeyboardState.IsKeyUp(Keys.D7))
            {
                CurrentTexture = SaintTexture;
            }

            if (keyboardState.IsKeyDown(Keys.D8) && previousKeyboardState.IsKeyUp(Keys.D8))
            {
                CurrentTexture = ScienceTexture;
            }
            if (keyboardState.IsKeyDown(Keys.D9) && previousKeyboardState.IsKeyUp(Keys.D9))
            {
                CurrentTexture = SquareTexture;
            }
            #endregion

            #region Technique Switching (F1–F5)
            // F1: Visualize tangent-space normal map as RGB
            if (keyboardState.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1))
            {
                effect.CurrentTechnique = effect.Techniques["VisualizeNormalMap"];
            }
            // F2: Visualize bump normals in world space as RGB
            if (keyboardState.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2))
            {
                effect.CurrentTechnique = effect.Techniques["VisualizeWorldNormal"];
            }
            // F3: Tangent-space bump mapping
            if (keyboardState.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3))
            {
                effect.CurrentTechnique = effect.Techniques["BumpMapping"];
            }
            // F4: Reflective bump mapping
            if (keyboardState.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4))
            {
                effect.CurrentTechnique = effect.Techniques["ReflectiveBump"];
            }
            // F5: Refractive bump mapping
            if (keyboardState.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5))
            {
                effect.CurrentTechnique = effect.Techniques["RefractiveBump"];
            }
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

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            GraphicsDevice.SetVertexBuffer(null);
            GraphicsDevice.Indices = null;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in torusModel.Meshes)
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
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);

                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["normalMap"].SetValue(CurrentTexture);

                        // Mip-map toggle
                        effect.Parameters["UseMipMap"].SetValue(useMipMap);

                        // Refraction index
                        effect.Parameters["RefractionIndex"].SetValue(refractionIndex);

                        // Bump tiling and height
                        effect.Parameters["BumpTilingU"].SetValue(bumpTilingU);
                        effect.Parameters["BumpTilingV"].SetValue(bumpTilingV);
                        effect.Parameters["BumpHeight"].SetValue(bumpHeight);

                        // Pass environment map for reflective/refractive techniques
                        string techniqueName = effect.CurrentTechnique.Name;
                        if (techniqueName == "ReflectiveBump" || techniqueName == "RefractiveBump")
                        {
                            effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);
                        }

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

            DrawUIStateManagement();

            base.Draw(gameTime);
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
            string helpText = "=== CONTROLS ===\n\n" +
                            "CAMERA:\n" +
                            "  Left Mouse Drag: Rotate camera\n" +
                            "  Right Mouse Drag: Zoom in/out\n" +
                            "  Middle Mouse Drag: Pan camera\n" +
                            "  S: Reset camera and light\n\n" +
                            "LIGHT:\n" +
                            "  Arrow Keys: Rotate light\n\n" +
                            "BUMP PARAMETERS:\n" +
                            "  U / Shift+U: Increase / Decrease bump tiling U\n" +
                            "  V / Shift+V: Increase / Decrease bump tiling V\n" +
                            "  W / Shift+W: Increase / Decrease bump height\n" +
                            "  M: Toggle mip-mapping ON / OFF\n\n" +
                            "NORMAL MAPS:\n" +
                            "  1-9: Switch normal maps\n\n" +
                            "SHADERS:\n" +
                            "  F1: Visualize tangent-space normal map (RGB)\n" +
                            "  F2: Visualize world-space normals (RGB)\n" +
                            "  F3: Bump mapping (diffuse + specular)\n" +
                            "  F4: Reflective bump mapping\n" +
                            "  F5: Refractive bump mapping\n\n" +
                            "UI:\n" +
                            "  ?: Toggle this help screen\n" +
                            "  H: Toggle info display";

            _spriteBatch.DrawString(font, helpText, new Vector2(10, 10), Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }

        private void DrawInfoDisplay()
        {
            string shaderName = effect.CurrentTechnique.Name;

            string infoText = "=== SHADER INFO ===\n\n" +
                            $"Shader: {shaderName}\n\n" +
                            $"Camera Angle: ({angle:F2}, {angle2:F2})\n" +
                            $"Camera Distance: {distance:F2}\n\n" +
                            $"Light Angle: ({angleL:F2}, {angleL2:F2})\n" +
                            $"Light Intensity: {lightIntensity:F2}\n\n" +
                            $"Shininess: {shininess:F1}\n" +
                            $"Specular Intensity: {specularIntensity:F2}\n\n" +
                            $"Bump Tiling U: {bumpTilingU:F2}\n" +
                            $"Bump Tiling V: {bumpTilingV:F2}\n" +
                            $"Bump Height: {bumpHeight:F2}\n\n" +
                            $"Mip-Map: {(useMipMap ? "ON" : "OFF")}";

            // Add refraction-specific info
            if (shaderName == "RefractiveBump")
            {
                infoText += $"\nRefraction Index: {refractionIndex:F3}";
            }

            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width - 350, 10);
            _spriteBatch.DrawString(font, infoText, position, Color.Yellow, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }
    }
}
