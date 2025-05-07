using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Net.WebSockets;

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

        public Vector3 lastRay;

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

        public bool Raycast() {
            return Raycast(cameraPosition, cameraR, cameraT);
        }

        public bool Raycast(Vector3 O, float R, float T) {
            Vector3 D = new Vector3(
                (float)(Math.Cos(T) * Math.Sin(R)),
                (float)(Math.Sin(T)),
                (float)(Math.Cos(T) * Math.Cos(R))
            );
            D.Normalize();
            return Raycast(O, D);
        }

        public bool Raycast(Vector3 O, Vector3 D) {
            float closest = float.MaxValue;
            D.X = -D.X;
            D.Z = -D.Z;
            for (int i = 0; i < indices.Length; i += 6) {

                // Triangle 1
                float? r1 = RayIntersectsTriangle(O, D,
                    vertices[indices[i]].Position,
                    vertices[indices[i + 1]].Position,
                    vertices[indices[i + 2]].Position,
                    out Vector3 hit1);
                if (r1 != null) {
                    if (r1 < closest) {
                        lastRay = hit1;
                        closest = (float)r1;
                    }
                }

                // Triangle 2
                float? r2 = RayIntersectsTriangle(O, D,
                    vertices[indices[i + 3]].Position,
                    vertices[indices[i + 4]].Position,
                    vertices[indices[i + 5]].Position,
                    out Vector3 hit2);
                if (r2 != null) {
                    if(r2 < closest) {
                        lastRay = hit2;
                        closest = (float)r2;
                    }
                }
            }

            if(closest < float.MaxValue) {
                return true;
            }
            return false;
        }

        private float? RayIntersectsTriangle(Vector3 O, Vector3 D, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 hitPoint) {
            hitPoint = Vector3.Zero;

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 h = Vector3.Cross(D, edge2);
            float a = Vector3.Dot(edge1, h);

            if (Math.Abs(a) < 0.000001f) return null;

            float f = 1f / a;
            Vector3 s = O - v0;
            float u = f * Vector3.Dot(s, h);
            if (u < 0 || u > 1) return null;

            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(D, q);
            if (v < 0 || u + v > 1) return null;

            float t = f * Vector3.Dot(edge2, q);
            if (t > 0.000001f) {
                hitPoint = O + t * D;
                return t;
            }

            return null;
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