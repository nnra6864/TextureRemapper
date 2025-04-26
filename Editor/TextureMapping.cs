using System.Collections.Generic;
using UnityEngine;

namespace Assets.NnUtils.Modules.TextureRemapper.Editor
{
    [System.Serializable]
    public class TextureMapping
    {
        public Texture2D Texture;
        public List<ChannelMapping> ChannelMappings = new();

        public TextureMapping(Texture2D texture = null)
        {
            Texture = texture;
            ChannelMappings.Add(new());
        }
    }
}