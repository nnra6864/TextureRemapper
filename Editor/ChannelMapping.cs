#if UNITY_EDITOR

namespace NnUtils.Modules.TextureRemapper.Editor
{
    [System.Serializable]
    public class ChannelMapping
    {
        public int InputChannel;
        public bool Invert;
        public int OutputChannel;
    }
}

#endif