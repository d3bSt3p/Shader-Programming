using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// import Library
using CPI411.SimpleEngine;

namespace Assignment02
{
    public class Assignment02 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        // Shader effect
        Effect effect;

        // Lab 3
        Model boxModel;
        Model sphereModel;
        Model torusModel;
        Model teapotModel;
        Model bunnyModel;
        Model helicopterModel;

        Model currentModel;

        // Lab 6
        Texture2D helicopterTexture;

        // Lab 2
        Matrix world;
        Matrix view;
        Matrix projection;

        // lab 3
        float angle = 0; 
        float angle2 = 0; 
        float distance = 15.0f;
       
        private MouseState previousMouseState;

        private Vector3 cameraPosition;

        // Lab 4
        float shininess = 20.0f;

        // Lab 4
        int currentTechnique = 0; 
        private KeyboardState previousKeyboardState;


        // Assignment 1
        // Camera translation
        Vector3 cameraOffset = Vector3.Zero;

        // Light rotation
        float lightAngleY = 0.8f;
        float lightAngleX = 0.6f;
        
        // Default values camera and light reset
        float defaultAngle = 0;
        float defaultAngle2 = 0;
        float defaultDistance = 15.0f;
        Vector3 defaultCameraOffset = Vector3.Zero;
        float defaultLightAngleY = 0.8f;
        float defaultLightAngleX = 0.6f;

        // Light properties
        float lightIntensity = 1.0f;
        float lightRed = 1.0f;
        float lightGreen = 1.0f;
        float lightBlue = 1.0f;
        float specularIntensity = 1.0f;

        // Refraction parameters
        float refractionIndex = 0.66f;
        float displacementAmount = 0.1f;

        // Refraction dispersion parameters (eta ratios for RGB)
        float etaRatioRed = 0.95f;
        float etaRatioGreen = 1.0f;
        float etaRatioBlue = 1.05f;

        // Fresnel parameters
        float fresnelBias = 0.1f;
        float fresnelScale = 0.9f;
        float fresnelPower = 5.0f;

        // UI display toggles
        bool showHelp = false;
        bool showInfo = false;

        // Font for rendering text
        SpriteFont font;

        // Lab 5 - Skybox texture sets
        Skybox skybox;
        int currentSkyboxSet = 0;


        // Skybox texture paths

        // left  
        // right
        // top
        // bottom   
        // front 
        // back

        string[] skyboxTestColors =
        {
            "skybox/debug_negx", // left  
            "skybox/debug_posx", // right
            "skybox/debug_posy", // top
            "skybox/debug_negy", // bottom   
            "skybox/debug_posz", // front 
            "skybox/debug_negz"  // back
        };

        string[] skyboxOffice =
        {
            "skybox/nvlobby_new_negx", // left  
            "skybox/nvlobby_new_posx", // right
            "skybox/nvlobby_new_posy", //top
            "skybox/nvlobby_new_negy", // bottom 
            "skybox/nvlobby_new_negz", // front 
            "skybox/nvlobby_new_posz" // back
        };

        string[] skyboxDaytime =
        {
            "skybox/grandcanyon_negx", // left
            "skybox/grandcanyon_posx", // right
            "skybox/grandcanyon_posy", // top
            "skybox/grandcanyon_negy", // bottom
            "skybox/grandcanyon_negz", // front
            "skybox/grandcanyon_posz" // back
        };

        string[] skyboxSpace =
        {
            "skybox/indigo_lf", // left
            "skybox/indigo_rt", // right
            "skybox/indigo_up", // top
            "skybox/indigo_dn", // bottom
            "skybox/indigo_ft", // front
            "skybox/indigo_bk" // back
        };

        public Assignment02()
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
            
            // Load all models
            boxModel = Content.Load<Model>("3dModels/Box");
            sphereModel = Content.Load<Model>("3dModels/Sphere");
            torusModel = Content.Load<Model>("3dModels/Torus");
            teapotModel = Content.Load<Model>("3dModels/Teapot");
            bunnyModel = Content.Load<Model>("3dModels/bunnyUV");

            helicopterModel = Content.Load<Model>("3dModels/Helicopter");

            helicopterTexture = Content.Load<Texture2D>("3dModels/HelicopterTexture");

            // Lab 5 - Initialize with default skybox
            skybox = new Skybox(skyboxTestColors, Content, _graphics.GraphicsDevice);

            // Set default model to box
            currentModel = boxModel;

            // Load font for UI
            font = Content.Load<SpriteFont>("Font");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            // Lab 3
            // Rotate camera
            if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                angle += (currentMouseState.X - previousMouseState.X) * 0.01f;
                angle2 += (currentMouseState.Y - previousMouseState.Y) * 0.01f;
            }

            // Change distance
            if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Pressed)
            {
                distance += (currentMouseState.Y - previousMouseState.Y) * 0.1f;
            }

            // Assignment 1
            // Translate camera
            if (currentMouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Pressed)
            {
                // Calculate right and up vectors based on current camera rotation

                cameraOffset += Vector3.Transform(Vector3.Right, Matrix.CreateRotationY(angle)) * (currentMouseState.X - previousMouseState.X) * 0.01f;
                cameraOffset -= Vector3.Up * (currentMouseState.Y - previousMouseState.Y) * 0.01f; 
            }

            previousMouseState = currentMouseState;

            // Clamp distance to avoid going through the model, and flipping to the other side
            distance = MathHelper.Clamp(distance, 2.0f, 90.0f);

            // Rotate light
            float lightRotationSpeed = 0.05f;
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                lightAngleY -= lightRotationSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                lightAngleY += lightRotationSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                lightAngleX -= lightRotationSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                lightAngleX += lightRotationSpeed;
            }

            // Clamp light angle X to avoid flipping
            lightAngleX = MathHelper.Clamp(lightAngleX, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

            // Reset camera and light: "S" Key
            if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                angle = defaultAngle;
                angle2 = defaultAngle2;
                distance = defaultDistance;
                cameraOffset = defaultCameraOffset;
                lightAngleY = defaultLightAngleY;
                lightAngleX = defaultLightAngleX;
            }

            // Light intensity controls (L/l key with Shift modifier)
            bool isShiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            if (keyboardState.IsKeyDown(Keys.L) && previousKeyboardState.IsKeyUp(Keys.L))
            {
                if (isShiftPressed)
                {
                    lightIntensity -= 0.1f;
                }
                else
                {
                    lightIntensity += 0.1f;
                }
                lightIntensity = MathHelper.Clamp(lightIntensity, 0.0f, 5.0f);
            }

            // Reflectivity controls (+ / -)
            if (keyboardState.IsKeyDown(Keys.OemPlus) || keyboardState.IsKeyDown(Keys.Add))
            {
                specularIntensity += 0.05f;
                specularIntensity = MathHelper.Clamp(specularIntensity, 0.0f, 5.0f);
            }
            if (keyboardState.IsKeyDown(Keys.OemMinus) || keyboardState.IsKeyDown(Keys.Subtract))
            {
                specularIntensity -= 0.05f;
                specularIntensity = MathHelper.Clamp(specularIntensity, 0.0f, 5.0f);
            }

            // Fresnel Power controls (Q/q)
            if (keyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
            {
                if (isShiftPressed)
                {
                    fresnelPower -= 0.1f;
                }
                else
                {
                    fresnelPower += 0.1f;
                }
                fresnelPower = MathHelper.Clamp(fresnelPower, 0.1f, 20.0f);
            }

            // Fresnel Scale controls (W/w)
            if (keyboardState.IsKeyDown(Keys.W) && previousKeyboardState.IsKeyUp(Keys.W))
            {
                if (isShiftPressed)
                {
                    fresnelScale -= 0.01f;
                }
                else
                {
                    fresnelScale += 0.01f;
                }
                fresnelScale = MathHelper.Clamp(fresnelScale, 0.0f, 1.0f);
            }

            // Fresnel Bias controls (E/e)
            if (keyboardState.IsKeyDown(Keys.E) && previousKeyboardState.IsKeyUp(Keys.E))
            {
                if (isShiftPressed)
                {
                    fresnelBias -= 0.01f;
                }
                else
                {
                    fresnelBias += 0.01f;
                }
                fresnelBias = MathHelper.Clamp(fresnelBias, 0.0f, 1.0f);
            }

            // Refraction index controls (Alt + +/-)
            bool isAltPressed = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            if (isAltPressed)
            {
                if (keyboardState.IsKeyDown(Keys.OemPlus) || keyboardState.IsKeyDown(Keys.Add))
                {
                    refractionIndex += 0.01f;
                    refractionIndex = MathHelper.Clamp(refractionIndex, 0.5f, 2.0f);
                }
                if (keyboardState.IsKeyDown(Keys.OemMinus) || keyboardState.IsKeyDown(Keys.Subtract))
                {
                    refractionIndex -= 0.01f;
                    refractionIndex = MathHelper.Clamp(refractionIndex, 0.5f, 2.0f);
                }
            }

            // Eta ratio Red controls (R/r with Shift modifier)
            if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
            {
                if (isShiftPressed)
                {
                    etaRatioRed -= 0.01f;
                }
                else
                {
                    etaRatioRed += 0.01f;
                }
                etaRatioRed = MathHelper.Clamp(etaRatioRed, 0.5f, 2.0f);
            }

            // Eta ratio Green controls (G/g with Shift modifier)
            if (keyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G))
            {
                if (isShiftPressed)
                {
                    etaRatioGreen -= 0.01f;
                }
                else
                {
                    etaRatioGreen += 0.01f;
                }
                etaRatioGreen = MathHelper.Clamp(etaRatioGreen, 0.5f, 2.0f);
            }

            // Eta ratio Blue controls (B/b with Shift modifier)
            if (keyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
            {
                if (isShiftPressed)
                {
                    etaRatioBlue -= 0.01f;
                }
                else
                {
                    etaRatioBlue += 0.01f;
                }
                etaRatioBlue = MathHelper.Clamp(etaRatioBlue, 0.5f, 2.0f);
            }

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

            // Model switching with number keys
            if (keyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
            {
                currentModel = boxModel;
            }
            if (keyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
            {
                currentModel = sphereModel;
            }
            if (keyboardState.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
            {
                currentModel = torusModel;
            }
            if (keyboardState.IsKeyDown(Keys.D4) && previousKeyboardState.IsKeyUp(Keys.D4))
            {
                currentModel = teapotModel;
            }
            
            // the bunny model is not working, I have no idea how to fix it, I tried everything, so I commented it out to avoid the program crashing when trying to load the bunny model
            if (keyboardState.IsKeyDown(Keys.D5) && previousKeyboardState.IsKeyUp(Keys.D5))
            {
                currentModel = bunnyModel;
            }
            
            if (keyboardState.IsKeyDown(Keys.D6) && previousKeyboardState.IsKeyUp(Keys.D6))
            {
                currentModel = helicopterModel;
            }

            // Skybox switching with number keys (7-0)
            if (keyboardState.IsKeyDown(Keys.D7) && previousKeyboardState.IsKeyUp(Keys.D7))
            {
                currentSkyboxSet = 0;
                skybox = new Skybox(skyboxTestColors, Content, _graphics.GraphicsDevice);
            }
            if (keyboardState.IsKeyDown(Keys.D8) && previousKeyboardState.IsKeyUp(Keys.D8))
            {
                currentSkyboxSet = 1;
                skybox = new Skybox(skyboxOffice, Content, _graphics.GraphicsDevice);
            }
            if (keyboardState.IsKeyDown(Keys.D9) && previousKeyboardState.IsKeyUp(Keys.D9))
            {
                currentSkyboxSet = 2;
                skybox = new Skybox(skyboxDaytime, Content, _graphics.GraphicsDevice);
            }
            if (keyboardState.IsKeyDown(Keys.D0) && previousKeyboardState.IsKeyUp(Keys.D0))
            {
                currentSkyboxSet = 3;
                skybox = new Skybox(skyboxSpace, Content, _graphics.GraphicsDevice);
            }

            // Technique switching with Function keys (F1-F10)
            if (keyboardState.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1))
            {
                effect.CurrentTechnique = effect.Techniques["Gouraud"];
            }
            if (keyboardState.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2))
            {
                effect.CurrentTechnique = effect.Techniques["Phong"];
            }
            if (keyboardState.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3))
            {
                effect.CurrentTechnique = effect.Techniques["PhongBlinn"];
            }
            if (keyboardState.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4))
            {
                effect.CurrentTechnique = effect.Techniques["Schlick"];
            }
            if (keyboardState.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5))
            {
                effect.CurrentTechnique = effect.Techniques["Toon"];
            }
            if (keyboardState.IsKeyDown(Keys.F6) && previousKeyboardState.IsKeyUp(Keys.F6))
            {
                effect.CurrentTechnique = effect.Techniques["HalfLife"];
            }
            if (keyboardState.IsKeyDown(Keys.F7) && previousKeyboardState.IsKeyUp(Keys.F7))
            {
                effect.CurrentTechnique = effect.Techniques["Reflection"];
            }
            if (keyboardState.IsKeyDown(Keys.F8) && previousKeyboardState.IsKeyUp(Keys.F8))
            {
                effect.CurrentTechnique = effect.Techniques["Refraction"];
            }
            if (keyboardState.IsKeyDown(Keys.F9) && previousKeyboardState.IsKeyUp(Keys.F9))
            {
                effect.CurrentTechnique = effect.Techniques["RefractionDispersion"];
            }
            if (keyboardState.IsKeyDown(Keys.F10) && previousKeyboardState.IsKeyUp(Keys.F10))
            {
                effect.CurrentTechnique = effect.Techniques["Fresnel"];
            }

            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // New camera system that avoids gimbal lock
            Vector3 cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            Vector3 cameraUp = Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            view = Matrix.CreateLookAt(cameraPosition + cameraOffset, Vector3.Zero + cameraOffset, cameraUp);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            // Calculate light direction from angles
            Vector3 lightDirection = Vector3.Transform(
                Vector3.Forward,
                Matrix.CreateRotationX(lightAngleX) * Matrix.CreateRotationY(lightAngleY)
            );
            lightDirection = Vector3.Normalize(lightDirection);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            GraphicsDevice.SetVertexBuffer(null);  // Clear vertex buffer
            GraphicsDevice.Indices = null;         // Clear index buffer

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in currentModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["AmbientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                        effect.Parameters["AmbientIntensity"].SetValue(0.2f);
                        effect.Parameters["DiffuseLightDirection"].SetValue(lightDirection);
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(lightRed, lightGreen, lightBlue, 1.0f));
                        effect.Parameters["DiffuseIntensity"].SetValue(lightIntensity);

                        // Lab 4 specular parameters
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);

                        Matrix worldInverseTranspose =
                            Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        // Lab 6 - Set reflection parameters if using Reflection shader
                        if (effect.CurrentTechnique.Name == "Reflection")
                        {
                            effect.Parameters["decalMap"].SetValue(helicopterTexture);
                            effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);
                            // Only apply texture if using helicopter model
                            effect.Parameters["applyTexture"].SetValue(currentModel == helicopterModel);
                        }

                        // Refraction and Fresnel shader parameters
                        if (effect.CurrentTechnique.Name == "Refraction" || 
                            effect.CurrentTechnique.Name == "RefractionDispersion" ||
                            effect.CurrentTechnique.Name == "Fresnel")
                        {
                            effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);
                            effect.Parameters["RefractionIndex"].SetValue(refractionIndex);
                            effect.Parameters["DisplacementAmount"].SetValue(displacementAmount);
                        }

                        // Refraction Dispersion-specific parameters
                        if (effect.CurrentTechnique.Name == "RefractionDispersion")
                        {
                            effect.Parameters["EtaRatioRed"].SetValue(etaRatioRed);
                            effect.Parameters["EtaRatioGreen"].SetValue(etaRatioGreen);
                            effect.Parameters["EtaRatioBlue"].SetValue(etaRatioBlue);
                        }

                        // Fresnel-specific parameters
                        if (effect.CurrentTechnique.Name == "Fresnel")
                        {
                            effect.Parameters["FresnelBias"].SetValue(fresnelBias);
                            effect.Parameters["FresnelScale"].SetValue(fresnelScale);
                            effect.Parameters["FresnelPower"].SetValue(fresnelPower);
                        }

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

                // Reset graphics device states, so 3D rendering is not affected by the sprite batch
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            }

            base.Draw(gameTime);
        }

        private void DrawHelpScreen()
        {
            string helpText = "=== CONTROLS ===\n\n" +
                            "CAMERA:\n" +
                            "  Left Mouse Drag: Rotate camera\n" +
                            "  Right Mouse Drag: Zoom in/out\n" +
                            "  Middle Mouse Drag: Pan camera\n" +
                            "  S / s: Reset camera and light\n\n" +
                            "LIGHT:\n" +
                            "  Arrow Keys: Rotate light\n" +
                            "  L / l: Increase/Decrease light intensity\n\n" +
                            "MATERIAL:\n" +
                            "  + / -: Increase/Decrease reflectivity\n\n" +
                            "FRESNEL (F10):\n" +
                            "  Q / Shift+Q: Increase/Decrease Fresnel Power\n" +
                            "  W / Shift+W: Increase/Decrease Fresnel Scale\n" +
                            "  E / Shift+E: Increase/Decrease Fresnel Bias\n\n" +
                            "REFRACTION DISPERSION (F9):\n" +
                            "  Alt + / Alt -: Increase/Decrease refraction index\n" +
                            "  R / Shift+R: Increase/Decrease Red eta ratio\n" +
                            "  G / Shift+G: Increase/Decrease Green eta ratio\n" +
                            "  B / Shift+B: Increase/Decrease Blue eta ratio\n\n" +
                            "MODELS:\n" +
                            "  1-4: Switch models (Box/Sphere/Torus/Teapot)\n" +
                            "  6: Switch to textured helicopter\n\n" +
                            "SKYBOX:\n" +
                            "  7: Switch to test color skybox\n" +
                            "  8: Switch to skybox set A\n" +
                            "  9: Switch to skybox set B\n" +
                            "  0: Switch to skybox set C\n\n" +
                            "SHADERS:\n" +
                            "  F1-F10: Switch shader technique\n\n" +
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
                            $"Light Angle: ({lightAngleY:F2}, {lightAngleX:F2})\n" +
                            $"Light Intensity: {lightIntensity:F2}\n\n" +
                            $"Reflectivity: {specularIntensity:F2}";

            // Add refraction-specific info
            if (shaderName == "Refraction" || shaderName == "RefractionDispersion" || shaderName == "Fresnel")
            {
                infoText += $"\n\nRefraction Index: {refractionIndex:F3}";
            }

            // Add dispersion-specific info
            if (shaderName == "RefractionDispersion")
            {
                infoText += $"\n\nEta Ratios (RGB):\n" +
                           $"  Red:   {etaRatioRed:F3}\n" +
                           $"  Green: {etaRatioGreen:F3}\n" +
                           $"  Blue:  {etaRatioBlue:F3}";
            }

            // Add Fresnel-specific info
            if (shaderName == "Fresnel")
            {
                infoText += $"\n\nFresnel Parameters:\n" +
                           $"  Bias:  {fresnelBias:F3}\n" +
                           $"  Scale: {fresnelScale:F3}\n" +
                           $"  Power: {fresnelPower:F2}";
            }

            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width - 350, 10);
            _spriteBatch.DrawString(font, infoText, position, Color.Yellow, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
        }
    }
}
