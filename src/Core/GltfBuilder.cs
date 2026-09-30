using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KimeraCS
{
    //
    // Minimal glTF 2.0 writer (.gltf + .bin).
    //
    // Layout rules follow FFNx's gltf loader (src/external_mesh.cpp), which reads the bufferView
    // offset but ignores accessor byteOffset and bufferView byteStride:
    //   - every accessor gets its own bufferView, tightly packed, accessor byteOffset 0
    //   - bufferViews are aligned to 4 bytes
    //
    public class GltfBuilder
    {
        public const int GL_UNSIGNED_BYTE = 5121;
        public const int GL_UNSIGNED_SHORT = 5123;
        public const int GL_UNSIGNED_INT = 5125;
        public const int GL_FLOAT = 5126;

        public const int TARGET_ARRAY_BUFFER = 34962;
        public const int TARGET_ELEMENT_ARRAY_BUFFER = 34963;

        public readonly JsonArray Nodes = new JsonArray();
        public readonly JsonArray Meshes = new JsonArray();
        public readonly JsonArray Materials = new JsonArray();
        public readonly JsonArray Textures = new JsonArray();
        public readonly JsonArray Images = new JsonArray();
        public readonly JsonArray Samplers = new JsonArray();
        public readonly JsonArray Skins = new JsonArray();
        public readonly JsonArray Animations = new JsonArray();
        public readonly JsonArray Accessors = new JsonArray();
        public readonly JsonArray BufferViews = new JsonArray();
        public readonly JsonArray SceneNodes = new JsonArray();
        public readonly List<string> ExtensionsUsed = new List<string>();

        private readonly MemoryStream binData = new MemoryStream();

        public int AddNode(JsonObject node)
        {
            Nodes.Add(node);
            return Nodes.Count - 1;
        }

        private static int Add(JsonArray array, JsonObject item)
        {
            array.Add(item);
            return array.Count - 1;
        }

        public int AddMesh(JsonObject mesh) => Add(Meshes, mesh);
        public int AddMaterial(JsonObject material) => Add(Materials, material);
        public int AddTexture(JsonObject texture) => Add(Textures, texture);
        public int AddImage(JsonObject image) => Add(Images, image);
        public int AddSampler(JsonObject sampler) => Add(Samplers, sampler);
        public int AddSkin(JsonObject skin) => Add(Skins, skin);
        public int AddAnimation(JsonObject animation) => Add(Animations, animation);

        private int AddBufferView(byte[] data, int target)
        {
            // Align every bufferView start to 4 bytes (needed for float data).
            while (binData.Length % 4 != 0) binData.WriteByte(0);

            JsonObject view = new JsonObject
            {
                ["buffer"] = 0,
                ["byteLength"] = data.Length,
                ["byteOffset"] = (int)binData.Length,
            };
            if (target != 0) view["target"] = target;

            binData.Write(data, 0, data.Length);

            return Add(BufferViews, view);
        }

        private static int ComponentsOf(string type)
        {
            switch (type)
            {
                case "SCALAR": return 1;
                case "VEC2": return 2;
                case "VEC3": return 3;
                case "VEC4": return 4;
                case "MAT4": return 16;
                default: throw new ArgumentException("Unknown accessor type " + type);
            }
        }

        // Float accessor. withMinMax writes per-component min/max (required for POSITION and
        // animation input times).
        public int AddFloatAccessor(float[] data, string type, int target, bool withMinMax)
        {
            int n = ComponentsOf(type);
            byte[] bytes = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

            JsonObject acc = new JsonObject
            {
                ["bufferView"] = AddBufferView(bytes, target),
                ["componentType"] = GL_FLOAT,
                ["count"] = data.Length / n,
                ["type"] = type,
            };

            if (withMinMax && data.Length > 0)
            {
                JsonArray min = new JsonArray(), max = new JsonArray();
                for (int c = 0; c < n; c++)
                {
                    float lo = float.MaxValue, hi = float.MinValue;
                    for (int i = c; i < data.Length; i += n)
                    {
                        if (data[i] < lo) lo = data[i];
                        if (data[i] > hi) hi = data[i];
                    }
                    min.Add(lo);
                    max.Add(hi);
                }
                acc["min"] = min;
                acc["max"] = max;
            }

            return Add(Accessors, acc);
        }

        // Unsigned byte accessor (JOINTS_0 is read by FFNx as 4 unsigned bytes).
        public int AddByteAccessor(byte[] data, string type, int target)
        {
            JsonObject acc = new JsonObject
            {
                ["bufferView"] = AddBufferView(data, target),
                ["componentType"] = GL_UNSIGNED_BYTE,
                ["count"] = data.Length / ComponentsOf(type),
                ["type"] = type,
            };
            return Add(Accessors, acc);
        }

        // Index accessor: unsigned short when possible, unsigned int otherwise.
        public int AddIndexAccessor(int[] indices, int vertexCount)
        {
            byte[] bytes;
            int componentType;

            if (vertexCount <= 65535)
            {
                componentType = GL_UNSIGNED_SHORT;
                bytes = new byte[indices.Length * 2];
                for (int i = 0; i < indices.Length; i++)
                {
                    bytes[i * 2] = (byte)(indices[i] & 0xFF);
                    bytes[i * 2 + 1] = (byte)((indices[i] >> 8) & 0xFF);
                }
            }
            else
            {
                componentType = GL_UNSIGNED_INT;
                bytes = new byte[indices.Length * 4];
                Buffer.BlockCopy(indices, 0, bytes, 0, bytes.Length);
            }

            JsonObject acc = new JsonObject
            {
                ["bufferView"] = AddBufferView(bytes, TARGET_ELEMENT_ARRAY_BUFFER),
                ["componentType"] = componentType,
                ["count"] = indices.Length,
                ["type"] = "SCALAR",
            };
            return Add(Accessors, acc);
        }

        // Writes <folder>\<baseName>.gltf and <folder>\<baseName>.bin
        public void Save(string folder, string baseName, string generator)
        {
            while (binData.Length % 4 != 0) binData.WriteByte(0);

            string binName = baseName + ".bin";

            JsonObject root = new JsonObject
            {
                ["asset"] = new JsonObject { ["generator"] = generator, ["version"] = "2.0" },
            };

            if (ExtensionsUsed.Count > 0)
            {
                JsonArray ext = new JsonArray();
                foreach (string e in ExtensionsUsed) ext.Add(e);
                root["extensionsUsed"] = ext;
            }

            root["scene"] = 0;
            root["scenes"] = new JsonArray { new JsonObject { ["name"] = "Scene", ["nodes"] = SceneNodes } };

            void Put(string key, JsonArray arr) { if (arr.Count > 0) root[key] = arr; }

            Put("nodes", Nodes);
            Put("animations", Animations);
            Put("materials", Materials);
            Put("meshes", Meshes);
            Put("textures", Textures);
            Put("images", Images);
            Put("skins", Skins);
            Put("accessors", Accessors);
            Put("bufferViews", BufferViews);
            Put("samplers", Samplers);

            root["buffers"] = new JsonArray
            {
                new JsonObject { ["byteLength"] = (int)binData.Length, ["uri"] = binName },
            };

            JsonSerializerOptions jso = new JsonSerializerOptions { WriteIndented = true };

            File.WriteAllBytes(Path.Combine(folder, binName), binData.ToArray());
            File.WriteAllText(Path.Combine(folder, baseName + ".gltf"), root.ToJsonString(jso),
                              new UTF8Encoding(false));
        }
    }
}
