using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab1
{
    public class Lab01 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

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

        public Lab01()
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
            _effect = Content.Load<Effect>("SimplestShader");

            // 
            _effect.Parameters["MyTexture"].SetValue(Content.Load<Texture2D>("logo_mg"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

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
