using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CPI411.SimpleEngine
{
    public class Skybox
    {
        private Model skyBox;
        public TextureCube skyBoxTexture;
        private Effect skyBoxEffect;
        private float size = 50f;

        public Skybox(string[] skyboxTextures, ContentManager Content, GraphicsDevice g)
        {
            skyBox = Content.Load<Model>("skybox/cube");
            skyBoxEffect = Content.Load<Effect>("skybox/Skybox");

            // Load the first texture to determine size
            Texture2D firstTexture = Content.Load<Texture2D>(skyboxTextures[0]);
            int textureSize = firstTexture.Width; 

            skyBoxTexture = new TextureCube(g, textureSize, false, SurfaceFormat.Color);
            byte[] data = new byte[textureSize * textureSize * 4];

            Texture2D tempTexture = Content.Load<Texture2D>(skyboxTextures[0]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.NegativeX, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[1]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.PositiveX, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[2]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.PositiveY, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[3]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.NegativeY, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[4]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.NegativeZ, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[5]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.PositiveZ, data);


        }
        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            foreach (EffectPass pass in skyBoxEffect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in skyBox.Meshes)
                {

                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = skyBoxEffect;
                        part.Effect.Parameters["World"].SetValue(
                        Matrix.CreateScale(size) *
                       Matrix.CreateTranslation(cameraPosition));
                        part.Effect.Parameters["View"].SetValue(view);
                        part.Effect.Parameters["Projection"].SetValue(projection);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(skyBoxTexture);
                        part.Effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                    }
                    mesh.Draw();

                }
                   
                }
            }
        }
    }
