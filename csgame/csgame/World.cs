using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public List<Block> blocks = new List<Block>();
        public bool regenMap = false;

        public Vector3 lastRayPos;
        public int lastRayIndex;

        public int selectedBlock;

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
            selectedBlock = -1;

            float size = 100f;
            for (int i = 0; i < 10000; i++) {
                int num = rnd.Next(3);
                if (num == 0) {
                    blocks.Add(new Block(new Vector3((rnd.NextSingle() - 0.5f) * size, (rnd.NextSingle() - 0.5f) * size, -((rnd.NextSingle()) * size)), new Vector3(1f, 1f, 1f), "grass"));
                } else if (num == 1) {
                    blocks.Add(new Block(new Vector3((rnd.NextSingle() - 0.5f) * size, (rnd.NextSingle() - 0.5f) * size, -((rnd.NextSingle()) * size)), new Vector3(1f, 1f, 1f), "dirt"));
                } else if (num == 2) {
                    blocks.Add(new Block(new Vector3((rnd.NextSingle() - 0.5f) * size, (rnd.NextSingle() - 0.5f) * size, -((rnd.NextSingle()) * size)), new Vector3(1f, 1f, 1f), "stone"));
                }
            }
            GenerateMap();
        }

        public void AddBlock(Vector3 pos, Vector3 size, string tex) {
            blocks.Add(new Block(pos, size, tex));
            regenMap = true;
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
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(HX, HY));
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
                UVs.Add(new Vector2(LX, LY));
                UVs.Add(new Vector2(LX, HY));
                UVs.Add(new Vector2(HX, HY));
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

        /// <summary>
        /// Raycasts from the camera position forwards onto a specified plane
        /// </summary>
        /// <param name="c1">One corner of the plane</param>
        /// <param name="c2">Opposite corner of the plane</param>
        /// <returns>true/false if the ray hit, position is stored in lastRay property</returns>
        public bool RaycastPlane(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4) {
            return RaycastPlane(c1, c2, c3, c4, cameraPosition, cameraR, cameraT);
        }

        /// <summary>
        /// Raycasts from a position onto a specified plane
        /// </summary>
        /// <param name="c1">One corner of the plane</param>
        /// <param name="c2">Opposite corner of the plane</param>
        /// <param name="O">Origin position</param>
        /// <param name="R">Yaw rotation (around Y axis, in radians)</param>
        /// <param name="T">Tilt rotation (around X axis, in radians)</param>
        /// <returns>true/false if the ray hit, position is stored in lastRay property</returns>
        public bool RaycastPlane(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, Vector3 O, float R, float T) {
            Vector3 D = new Vector3(
                (float)(Math.Cos(T) * Math.Sin(R)),
                (float)(Math.Sin(T)),
                (float)(Math.Cos(T) * Math.Cos(R))
            );
            D.Normalize();
            return RaycastPlane(c1, c2, c3, c4, O, D);
        }

        /// <summary>
        /// Raycasts from a position onto a specified plane
        /// </summary>
        /// <param name="c1">One corner of the plane</param>
        /// <param name="c2">Opposite corner of the plane</param>
        /// <param name="O">Origin position</param>
        /// <param name="D">Direction vector (in radians)</param>
        /// <returns>true/false if the ray hit, position is stored in lastRay property</returns>
        public bool RaycastPlane(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, Vector3 O, Vector3 D) {

            // Triangle 1
            float? r1 = RayIntersectsTriangle(O, D, c1, c2, c3, out Vector3 hit1);
            if (r1 != null) {
                lastRayPos = hit1;
                return true;
            }

            // Triangle 2
            float? r2 = RayIntersectsTriangle(O, D, c1, c3, c4, out Vector3 hit2);
            if (r2 != null) {
                lastRayPos = hit2;
                return true;
            }

            return false;
        }

        public bool Raycast() {
            Stopwatch sw = Stopwatch.StartNew();
            bool temp = Raycast(cameraPosition, cameraR, cameraT);
            sw.Stop();
            Debug.WriteLine($"{sw.ElapsedMilliseconds}ms");
            return temp;
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
                        lastRayPos = hit1;
                        lastRayIndex = i / 36;
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
                    if (r2 < closest) {
                        lastRayPos = hit2;
                        lastRayIndex = i / 36;
                        closest = (float)r2;
                    }
                }
            }

            if (closest < float.MaxValue) {
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

            if (regenMap)
                GenerateMap();

            regenMap = false;

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