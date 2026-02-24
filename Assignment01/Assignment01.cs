using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Assignment01
{
    public class Assignment01 : Game
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
        Model currentModel;

        // Lab 2
        Matrix world;
        Matrix view;
        Matrix projection;

        // lab 3
        float angle = 0; 
        float angle2 = 0; 
        float distance = 15.0f;
       
        private MouseState previousMouseState;

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

        // UI display toggles
        bool showHelp = false;
        bool showInfo = false;

        // Font for rendering text
        SpriteFont font;
            
        public Assignment01()
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
            bunnyModel = Content.Load<Model>("3dModels/Bunny");
            
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
            distance = MathHelper.Clamp(distance, 10.0f, 90.0f);

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

            // Light red component (R/r key with Shift modifier)
            if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
            {
                if (isShiftPressed)
                {
                    lightRed -= 0.1f;
                }
                else
                {
                    lightRed += 0.1f;
                }
                lightRed = MathHelper.Clamp(lightRed, 0.0f, 1.0f);
            }

            // Light green component (G/g key with Shift modifier)
            if (keyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G))
            {
                if (isShiftPressed)
                {
                    lightGreen -= 0.1f;
                }
                else
                {
                    lightGreen += 0.1f;
                }
                lightGreen = MathHelper.Clamp(lightGreen, 0.0f, 1.0f);
            }

            // Light blue component (B/b key with Shift modifier)
            if (keyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
            {
                if (isShiftPressed)
                {
                    lightBlue -= 0.1f;
                }
                else
                {
                    lightBlue += 0.1f;
                }
                lightBlue = MathHelper.Clamp(lightBlue, 0.0f, 1.0f);
            }

            // Specular intensity controls (+/- keys without Control)
            bool isControlPressed = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
            if (!isControlPressed)
            {
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
            }

            // Shininess controls (Left Control  +/-)
            if (isControlPressed)
            {
                if (keyboardState.IsKeyDown(Keys.OemPlus) || keyboardState.IsKeyDown(Keys.Add))
                {
                    shininess += 1.0f;
                }
                if (keyboardState.IsKeyDown(Keys.OemMinus) || keyboardState.IsKeyDown(Keys.Subtract))
                {
                    shininess -= 1.0f;
                }
                shininess = MathHelper.Clamp(shininess, 1.0f, 200.0f);
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
            if (keyboardState.IsKeyDown(Keys.D5) && previousKeyboardState.IsKeyUp(Keys.D5))
            {
                currentModel = bunnyModel;
            }

            // Technique switching with Function keys (F1-F6)
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
                            "  L / l: Increase/Decrease light intensity\n" +
                            "  R / r: Increase/Decrease red component\n" +
                            "  G / g: Increase/Decrease green component\n" +
                            "  B / b: Increase/Decrease blue component\n\n" +
                            "MATERIAL:\n" +
                            "  + / -: Increase/Decrease specular intensity\n" +
                            "  Ctrl + / Ctrl -: Increase/Decrease shininess\n\n" +
                            "MODELS:\n" +
                            "  1-5: Switch models (Box/Sphere/Torus/Teapot/Bunny)\n\n" +
                            "SHADERS:\n" +
                            "  F1-F6: Switch shader technique\n\n" +
                            "UI:\n" +
                            "  ?: Toggle this help screen\n" +
                            "  H: Toggle info display";

            _spriteBatch.DrawString(font, helpText, new Vector2(10, 10), Color.White);
        }

        private void DrawInfoDisplay()
        {
            string shaderName = effect.CurrentTechnique.Name;

            string infoText = "=== SHADER INFO ===\n\n" +
                            $"Shader: {shaderName}\n\n" +
                            $"Camera Angle: ({angle:F2}, {angle2:F2})\n" +
                            $"Camera Distance: {distance:F2}\n\n" +
                            $"Light Angle: ({lightAngleY:F2}, {lightAngleX:F2})\n" +
                            $"Light Intensity: {lightIntensity:F2}\n" +
                            $"Light RGB: ({lightRed:F2}, {lightGreen:F2}, {lightBlue:F2})\n\n" +
                            $"Specular Intensity: {specularIntensity:F2}\n" +
                            $"Shininess: {shininess:F1}";

            
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width - 350, 10);
            _spriteBatch.DrawString(font, infoText, position, Color.Yellow);
        }
    }
}
