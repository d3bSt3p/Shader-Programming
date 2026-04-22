using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using System;
namespace WaterRenderingDemo
{
    public class WaterGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Shader effect
        Effect effect;

        // lab 7
        SpriteFont font;

        Texture2D normalMap;

        // Lab 2
        Matrix world;
        Matrix view;
        Matrix projection;

        // lab 3 exercise
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

        // ** Water Rendering Final
        Texture2D heightMap;
        Texture2D heightMap2;
        Texture2D heightMap3;
        Texture2D heightMap4;
        float totalTime = 0f;
        float waveAmplitude = 2f;
        float waveSpeed = 0.02f;
        float windAngle = MathF.Atan2(0.6f, 1f); 
        float waterLevel = 0f;
        float textureScale = 1f;
        float normalMapScale = 1f;
        float waterAlpha = 0.1f;
        float shininess = 50f;
        Vector2 windDirection = new Vector2(1f, 0.6f);

        bool showHelp = false;

        // Procedural water grid
        VertexBuffer waterVertexBuffer;
        IndexBuffer waterIndexBuffer;
        int waterPrimitiveCount;

        // Grid resolution
        const int GridSize = 200;
        const float GridSpacing = 0.2f;

        public WaterGame()
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
            font = Content.Load<SpriteFont>("Font");
            effect = Content.Load<Effect>("Water");
            heightMap = Content.Load<Texture2D>("DisplacementHeightMaps/DisplacementHeightMap");
            heightMap2 = Content.Load<Texture2D>("DisplacementHeightMaps/DisplacementHeightMap2");
            heightMap3 = Content.Load<Texture2D>("DisplacementHeightMaps/DisplacementHeightMap3");
            heightMap4 = Content.Load<Texture2D>("DisplacementHeightMaps/DisplacementHeightMap4");
            normalMap = Content.Load<Texture2D>("underwaterNormal");

            BuildWaterGrid();
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

            if (mouseState.MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                cameraTarget -= ViewRight * (mouseState.X - previousMouseState.X) / 10f;
                cameraTarget += ViewUp * (mouseState.Y - previousMouseState.Y) / 10f;
            }

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) *
                Matrix.CreateTranslation(cameraTarget));

            view = Matrix.CreateLookAt(cameraPosition, cameraTarget,
                Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));

            distance = MathHelper.Clamp(distance, 0.1f, 90.0f);

            if (keyboardState.IsKeyDown(Keys.OemPlus)) waveAmplitude += 0.01f;
            if (keyboardState.IsKeyDown(Keys.OemMinus)) waveAmplitude -= 0.01f;
            waveAmplitude = MathHelper.Clamp(waveAmplitude, 0f, 10f);

            if (keyboardState.IsKeyDown(Keys.OemOpenBrackets))  waterAlpha -= 0.01f;
            if (keyboardState.IsKeyDown(Keys.OemCloseBrackets)) waterAlpha += 0.01f;
            waterAlpha = MathHelper.Clamp(waterAlpha, 0f, 1f);                         

            // Wave speed — W increases, S decreases
            if (keyboardState.IsKeyDown(Keys.W)) waveSpeed += 0.001f;
            if (keyboardState.IsKeyDown(Keys.S)) waveSpeed -= 0.001f;
            waveSpeed = MathHelper.Clamp(waveSpeed, 0f, 0.1f);

            // Wind direction — A rotates counter-clockwise, D rotates clockwise
            if (keyboardState.IsKeyDown(Keys.A)) windAngle -= 0.01f;
            if (keyboardState.IsKeyDown(Keys.D)) windAngle += 0.01f;
            windDirection = new Vector2(MathF.Cos(windAngle), MathF.Sin(windAngle));

            // Normal map scale — Y increases, H decreases
            if (keyboardState.IsKeyDown(Keys.Y)) normalMapScale += 0.01f;
            if (keyboardState.IsKeyDown(Keys.H)) normalMapScale -= 0.01f;
            normalMapScale = MathHelper.Clamp(normalMapScale, 0.1f, 10f);

            //toggle debugging info
            if (keyboardState.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
            {
                showHelp = !showHelp;
            }

            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;

            totalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            Matrix worldMatrix = Matrix.Identity;
            Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldMatrix));

            GraphicsDevice.BlendState = BlendState.AlphaBlend;  
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            effect.CurrentTechnique = effect.Techniques[0];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                effect.Parameters["World"].SetValue(worldMatrix);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                effect.Parameters["normalMap"].SetValue(normalMap);
                effect.Parameters["LightPosition"].SetValue(lightPosition);
                effect.Parameters["AmbientColor"].SetValue(1.0f);
                effect.Parameters["AmbientIntensity"].SetValue(0.1f);
                effect.Parameters["DiffuseColor"].SetValue(new Vector4(0.2f, 0.6f, 1f, 1f));
                effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
                effect.Parameters["SpecularColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
                effect.Parameters["SpecularIntensity"].SetValue(0.8f);
                effect.Parameters["Shininess"].SetValue(shininess);                              

                effect.Parameters["heightMap"].SetValue(heightMap);
                effect.Parameters["heightMap2"].SetValue(heightMap2);
                effect.Parameters["heightMap3"].SetValue(heightMap3);
                effect.Parameters["heightMap4"].SetValue(heightMap4);

                effect.Parameters["Time"].SetValue(totalTime);
                effect.Parameters["WaveAmplitude"].SetValue(waveAmplitude);
                effect.Parameters["WaveSpeed"].SetValue(waveSpeed);
                effect.Parameters["WaterLevel"].SetValue(waterLevel);
                effect.Parameters["TextureScale"].SetValue(textureScale);
                effect.Parameters["NormalMapScale"].SetValue(normalMapScale);
                effect.Parameters["WindDirection"].SetValue(windDirection);
                effect.Parameters["WaterAlpha"].SetValue(waterAlpha);       

                pass.Apply();

                GraphicsDevice.SetVertexBuffer(waterVertexBuffer);
                GraphicsDevice.Indices = waterIndexBuffer;

                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0, 0, waterPrimitiveCount);
            }
            if (showHelp)
            {
                ShowDebuggingInfo();
            }

            base.Draw(gameTime);
        }

        private void ShowDebuggingInfo()
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "Wave Amplitude: " + waveAmplitude.ToString("F2"),
                    new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(font, "Time: " + totalTime.ToString("F1"),
                new Vector2(10, 30), Color.White);
            _spriteBatch.DrawString(font, "+/- keys: change wave height",
                new Vector2(10, 50), Color.Yellow);
            _spriteBatch.DrawString(font, "[/] keys: change transparency  Alpha: " + waterAlpha.ToString("F2"),
                new Vector2(10, 70), Color.Yellow);
            _spriteBatch.DrawString(font, "W/S keys: change wave speed  Speed: " + waveSpeed.ToString("F3"),
                new Vector2(10, 90), Color.Cyan);
            _spriteBatch.DrawString(font, "A/D keys: rotate wind direction  Angle: " + MathHelper.ToDegrees(windAngle).ToString("F1"),
                new Vector2(10, 110), Color.Cyan);
            _spriteBatch.DrawString(font, "Y/H keys: normal map scale  Scale: " + normalMapScale.ToString("F2"),
                new Vector2(10, 130), Color.LightGreen);
            _spriteBatch.End();
        }

        private void BuildWaterGrid()
        {
            int vertexCount = (GridSize + 1) * (GridSize + 1);
            var vertices = new VertexPositionNormalTextureTangent[vertexCount];

            float origin = GridSize * GridSpacing * 0.5f;

            for (int z = 0; z <= GridSize; z++)
            {
                for (int x = 0; x <= GridSize; x++)
                {
                    int index = z * (GridSize + 1) + x;
                    float px = x * GridSpacing - origin;
                    float pz = z * GridSpacing - origin;

                    vertices[index] = new VertexPositionNormalTextureTangent
                    {
                        Position = new Vector3(px, 0f, pz),
                        Normal = Vector3.Up,
                        TextureCoordinate = new Vector2((float)x / GridSize, (float)z / GridSize),
                        Tangent = Vector3.UnitX,   
                        Binormal = Vector3.UnitZ    
                    };
                }
            }

            int indexCount = GridSize * GridSize * 6;
            var indices = new int[indexCount];
            int i = 0;

            for (int z = 0; z < GridSize; z++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    int topLeft = z * (GridSize + 1) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + (GridSize + 1);
                    int bottomRight = bottomLeft + 1;

                    // Triangle 1
                    indices[i++] = topLeft;
                    indices[i++] = bottomLeft;
                    indices[i++] = topRight;

                    // Triangle 2
                    indices[i++] = topRight;
                    indices[i++] = bottomLeft;
                    indices[i++] = bottomRight;
                }
            }

            waterPrimitiveCount = indexCount / 3;

            waterVertexBuffer = new VertexBuffer(GraphicsDevice,
            VertexPositionNormalTextureTangent.VertexDeclaration,
            vertexCount, BufferUsage.WriteOnly);

            waterVertexBuffer.SetData(vertices);

            waterIndexBuffer = new IndexBuffer(GraphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                indexCount, BufferUsage.WriteOnly);
            waterIndexBuffer.SetData(indices);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormalTextureTangent : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 Binormal;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}

