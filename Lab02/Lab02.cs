using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab2
{
    public class Lab02 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Lab 2
        Matrix world = Matrix.Identity;

        // left and right changes the angle
        float angle = 0;

        // up and down changes the distance
        float distance = 2.0f;
             
        // Shader effect
        private Effect _effect;          

        // Define triangle vertices with position and texture coordinates
        VertexPositionTexture[] _vertices =
        {
            // Vertex positions (X, Y, Z) and texture coordinates (U, V)
            new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0.5f, 0)),
            new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(0, 1))
        };      

        public Lab02()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // statement needed to use the correct graphics profile
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

            // Load Effect
            _effect = Content.Load<Effect>("Simple3D");

            // Load Texture
            _effect.Parameters["MyTexture"].SetValue(Content.Load<Texture2D>("logo_mg"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // this whole chunk needs to be simplified
            // when the user presses left and right the camera angle is updated
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                angle += 0.02f;
                Vector3 offset = new Vector3
                    (
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle),
                    0);
                _effect.Parameters["offset"].SetValue(offset);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                angle -= 0.02f;
                Vector3 offset = new Vector3
                    (
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle),
                    0);
                _effect.Parameters["offset"].SetValue(offset);
            }

            // when the user presses up and down the camera distance is updated
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                distance -= 0.02f;
                Vector3 offset = new Vector3
                    (
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle),
                    0);
                _effect.Parameters["offset"].SetValue(offset);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                distance += 0.02f;
                Vector3 offset = new Vector3
                    (
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle),
                    0);
                _effect.Parameters["offset"].SetValue(offset);
            }

            // shoud be in the draw function
            // update the camera position using the angle and distance values
            Vector3 cameraPosition = distance * new Vector3(
            (float)System.Math.Sin(angle), 0, (float)System.Math.Cos(angle));

            // multiply these positions by the parameters
            Matrix view = Matrix.CreateLookAt(
                cameraPosition,
                new Vector3(),
                new Vector3(0, 1, 0));

            // Camera Projection
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            _effect.Parameters["World"].SetValue(world);
            _effect.Parameters["View"].SetValue(view);
            _effect.Parameters["Projection"].SetValue(projection);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Set the BlendState to AlphaBlend to make transparent colors
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Apply the effect and draw the triangle
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                // Apply the current pass of the effect
                pass.Apply();

                // DrawUserPrimitives to render the triangle
                // Primitive type,
                // array of vertex,
                // starting index,
                // number of polygons
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleList,
                    _vertices,
                    0,
                    _vertices.Length / 3
                );
            }

            base.Draw(gameTime);
        }
    }
}
