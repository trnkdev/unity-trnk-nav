using System;

namespace NekoNav.Internals
{
    internal struct Bitset
    {
        private const int BitsPerWord = 64;
        private const int BytesPerWord = sizeof(ulong);

        private ulong[] _words;
        private int _bitCount;

        public int BitCount => _bitCount;

        public void Resize(int bitCount)
        {
            _bitCount = bitCount;
            int wordCount = (bitCount + (BitsPerWord - 1)) / BitsPerWord;

            if (_words == null || _words.Length != wordCount)
                _words = new ulong[wordCount];
            else
                Array.Clear(_words, 0, _words.Length);
        }

        public void ClearAll()
        {
            if (_words == null) return;
            Array.Clear(_words, 0, _words.Length);
        }

        public bool Get(int index)
        {
            int word = index / BitsPerWord;
            int bit = index - (word * BitsPerWord);
            return ((_words[word] >> bit) & 1UL) != 0UL;
        }

        public void Set(int index, bool value)
        {
            int word = index / BitsPerWord;
            int bit = index - (word * BitsPerWord);
            ulong mask = 1UL << bit;

            if (value) _words[word] |= mask;
            else _words[word] &= ~mask;
        }

        public void CopyFrom(in Bitset other)
        {
            Resize(other._bitCount);
            if (other._words == null || other._words.Length == 0)
                return;

            Array.Copy(other._words, _words, _words.Length);
        }

        public byte[] ToBytes()
        {
            if (_words == null || _words.Length == 0)
                return Array.Empty<byte>();

            int byteLen = _words.Length * BytesPerWord;
            byte[] bytes = new byte[byteLen];
            Buffer.BlockCopy(_words, 0, bytes, 0, byteLen);
            return bytes;
        }

        public void FromBytes(int bitCount, byte[] bytes)
        {
            Resize(bitCount);

            if (bytes == null || bytes.Length == 0)
                return;

            int max = _words.Length * BytesPerWord;
            int len = bytes.Length <= max ? bytes.Length : max;
            Buffer.BlockCopy(bytes, 0, _words, 0, len);
        }
    }
}
