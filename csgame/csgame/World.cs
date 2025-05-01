using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace csgame {

    public static class ResourceManager {
        public static GraphicsDevice GraphicsDevice { get; set; }
    }

    public struct VertexCustom {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;

        public VertexCustom(Vector3 position, Vector3 normal, Vector2 texCoord) {
            Position = position;
            Normal = normal;
            TextureCoordinate = texCoord;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );
    }

    class World {

        private List<Block> blocks = new List<Block>();

        VertexCustom[] vertices;
        int[] indices;
        int primitivecount;

        public Effect lighting { get; set; }
        private static Random rnd = new Random();
        public Texture2D texture { get; set; }
        public Matrix projectionMatrix { get; set; }

        public Vector3 cameraPosition { get; set; }

        public float cameraR { get; set; }
        public float cameraT { get; set; }

        public World() {
            cameraPosition = new Vector3(0, 0, 0);
            cameraR = 0;
            cameraT = 0;
            for (int i = 0; i < 10000; i++) {
                blocks.Add(new Block(new Vector3((rnd.NextSingle() - 0.5f) * 100f, (rnd.NextSingle() - 0.5f) * 100f, -((rnd.NextSingle()) * 100f)), new Vector3(1f, 1f, 1f), "grass"));
            }
            GenerateMap();
        }

        public void GenerateMap() {
            Stopwatch sw = Stopwatch.StartNew();

            int primitiveCount = 0;

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<int> listIndices = new List<int>();
            int totalIndices = 0;
            foreach (var block in blocks) {

                float X = block.position.X;
                float Y = block.position.Y;
                float Z = block.position.Z;

                float LX = block.cLX;
                float LY = block.cLY;
                float HX = block.cHX;
                float HY = block.cHY;

                // Back (z-)
                positions.Add(new Vector3(X - 0.5f, Y - 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X - 0.5f, Y + 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y + 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y - 0.5f, Z - 0.5f));
                normals.Add(new Vector3(0, 0, -1));
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(HX, HY));
                UVs.Add(new Vector2(HX, LY));

                listIndices.AddRange(new int[] { 0 + totalIndices, 2 + totalIndices, 1 + totalIndices, 0 + totalIndices, 3 + totalIndices, 2 + totalIndices });

                totalIndices += 4;
                primitiveCount += 2;

                // Front (z+)
                positions.Add(new Vector3(X - 0.5f, Y - 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X - 0.5f, Y + 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y + 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y - 0.5f, Z + 0.5f));
                normals.Add(new Vector3(0, 0, 1));
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(HX, HY));
                UVs.Add(new Vector2(HX, LY));

                listIndices.AddRange(new int[] { 0 + totalIndices, 1 + totalIndices, 2 + totalIndices, 0 + totalIndices, 2 + totalIndices, 3 + totalIndices });

                totalIndices += 4;
                primitiveCount += 2;

                // Left (x-)
                positions.Add(new Vector3(X - 0.5f, Y + 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X - 0.5f, Y - 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X - 0.5f, Y - 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X - 0.5f, Y + 0.5f, Z - 0.5f));
                normals.Add(new Vector3(-1, 0, 0));
                UVs.Add(new Vector2(HX, HY));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(HX, LY));

                listIndices.AddRange(new int[] { 2 + totalIndices, 3 + totalIndices, 0 + totalIndices, 2 + totalIndices, 0 + totalIndices, 1 + totalIndices });

                totalIndices += 4;
                primitiveCount += 2;

                // Right (x+)
                positions.Add(new Vector3(X + 0.5f, Y + 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y - 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y - 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y + 0.5f, Z + 0.5f));
                normals.Add(new Vector3(1, 0, 0));
                UVs.Add(new Vector2(HX, HY));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(HX, LY));

                listIndices.AddRange(new int[] { 0 + totalIndices, 1 + totalIndices, 2 + totalIndices, 0 + totalIndices, 2 + totalIndices, 3 + totalIndices });

                totalIndices += 4;
                primitiveCount += 2;

                // Bottom (y-)
                positions.Add(new Vector3(X - 0.5f, Y - 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X - 0.5f, Y - 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y - 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y - 0.5f, Z - 0.5f));
                normals.Add(new Vector3(0, -1, 0));
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(HX, HY));
                UVs.Add(new Vector2(HX, LY));

                listIndices.AddRange(new int[] { 1 + totalIndices, 2 + totalIndices, 3 + totalIndices, 1 + totalIndices, 3 + totalIndices, 0 + totalIndices });

                totalIndices += 4;
                primitiveCount += 2;

                // Top (y+)
                positions.Add(new Vector3(X - 0.5f, Y + 0.5f, Z + 0.5f));
                positions.Add(new Vector3(X - 0.5f, Y + 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y + 0.5f, Z - 0.5f));
                positions.Add(new Vector3(X + 0.5f, Y + 0.5f, Z + 0.5f));
                normals.Add(new Vector3(0, 1, 0));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(HX, LY));
                UVs.Add(new Vector2(HX, HY));

                listIndices.AddRange(new int[] { 0 + totalIndices, 2 + totalIndices, 3 + totalIndices, 0 + totalIndices, 1 + totalIndices, 2 + totalIndices });

                totalIndices += 4;
                primitiveCount += 2;

            }

            // Other slop
            vertices = new VertexCustom[positions.Count];
            indices = listIndices.ToArray();

            for (int i = 0; i < positions.Count; i++) {
                vertices[i] = new VertexCustom(new Vector3(positions[i].X, positions[i].Y, positions[i].Z), new Vector3(normals[i / 4].X, normals[i / 4].Y, normals[i / 4].Z), new Vector2(UVs[i].X, UVs[i].Y));
            }

            sw.Stop();
            Debug.WriteLine($"{sw.ElapsedMilliseconds}ms > GenerateMap()");
        }

        public void render() {

            ResourceManager.GraphicsDevice.Clear(Color.SkyBlue);

            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(cameraR, cameraT, 0);
            Vector3 lookDirection = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 upDirection = Vector3.Transform(Vector3.Up, rotationMatrix);
            Matrix viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + lookDirection, upDirection);

            lighting.Parameters["World"].SetValue(Matrix.CreateTranslation(0, 0, 0));
            lighting.Parameters["playerPos"].SetValue(cameraPosition);

            lighting.Parameters["View"].SetValue(viewMatrix);
            lighting.Parameters["Projection"].SetValue(projectionMatrix);

            ResourceManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ResourceManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ResourceManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Point screenCenter = new Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

            ResourceManager.GraphicsDevice.SamplerStates[0] = new SamplerState { Filter = TextureFilter.Point, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap };

            foreach (var pass in lighting.CurrentTechnique.Passes) {
                pass.Apply();
                ResourceManager.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length) / 3, VertexCustom.VertexDeclaration);
            }

            ResourceManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            ResourceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            ResourceManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        }

    }
}