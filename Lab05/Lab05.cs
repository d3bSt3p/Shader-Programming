using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// import Library
using CPI411.SimpleEngine;

namespace Lab05
{
    public class Lab05 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        // Lab 2
        Matrix world;
        Matrix view;
        Matrix projection;


        // lab 3 exersise
        float angle = 0;
        float angle2 = 0;
        float distance = 15.0f;

        private MouseState previousMouseState;
        private Vector3 cameraPosition;


        // Lab 05
        Skybox skybox;
        string[] skyboxImageFiles = // 6 images located in skybox foler
        {
            "skybox/SunsetPNG1",
            "skybox/SunsetPNG2",
            "skybox/SunsetPNG3",
            "skybox/SunsetPNG4",
            "skybox/SunsetPNG5",
            "skybox/SunsetPNG6"
        };
        public Lab05()
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

            //********************************************************************
            skybox = new Skybox(skyboxImageFiles,Content,_graphics.GraphicsDevice);
            //********************************************************************
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add the key controls to rotate the camera

            // Lab 3
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                angle += (Mouse.GetState().X - previousMouseState.X) * 0.01f;
                angle2 += (Mouse.GetState().Y - previousMouseState.Y) * 0.01f;
            }

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                angle += 0.01f;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                angle -= 0.01f;
            }




            previousMouseState = Mouse.GetState();

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            Vector3 cameraUp = Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, cameraUp);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);

            //******************************************************
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None; // both sides are rendered
            GraphicsDevice.RasterizerState = rasterizerState;
            //******************************************************

            skybox.Draw(view, projection, cameraPosition);

            base.Draw(gameTime);
        }
    }
}
