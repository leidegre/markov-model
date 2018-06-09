using MarkovModel.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MarkovModel
{
    public class MarkovChain
    {
        public class Node
        {
            public Dictionary<string, int> _nodes;
            public int _total;

            public void Prepare()
            {
                int total = 0;

                foreach (var item in _nodes)
                {
                    total += item.Value;
                }

                _total = total;
            }

            public void WriteToStream(Stream outputStream, byte[] scratch)
            {
                var nodes = _nodes;

                outputStream.WriteVarInt63(nodes.Count);

                foreach (var item in nodes)
                {
                    var k = Encoding.UTF8.GetBytes(item.Key, 0, item.Key.Length, scratch, 0);

                    outputStream.WriteVarInt63(k);
                    outputStream.Write(scratch, 0, k);
                    outputStream.WriteVarInt63(item.Value);
                }
            }

            public void ReadFromStream(Stream inputStream, byte[] scratch)
            {
                var count = (int)inputStream.ReadVarInt63();

                var nodes = new Dictionary<string, int>(count);
                int total = 0;

                for (int i = 0; i < count; i++)
                {
                    var k = (int)inputStream.ReadVarInt63();
                    inputStream.Read(scratch, 0, k);
                    var k2 = Encoding.UTF8.GetString(scratch, 0, k);
                    var weight = (int)inputStream.ReadVarInt63();
                    nodes.Add(k2, weight);
                    total += weight;
                }

                _nodes = nodes;
                _total = total;
            }
        }

        public Dictionary<string, Node> _root;
        private int _k;

        public MarkovChain()
        {
        }

        public MarkovChain(int k)
        {
            if (!(1 < k)) throw new ArgumentOutOfRangeException();

            this._root = new Dictionary<string, Node>();
            this._k = k;
        }

        public IEnumerable<string> Tokenize(string text)
        {
            text = text + "\0"; // add terminal symbol

            var q = new Queue<char>();

            int i = 0, k = _k, l = text.Length;

            for (i = 0; i < l; i++)
            {
                if (!(q.Count < k))
                {
                    q.Dequeue();
                }

                q.Enqueue(text[i]);

                if (k == q.Count)
                {
                    yield return new string(q.ToArray());
                }
            }

            if (q.Count % k != 0)
            {
                yield return new string(q.ToArray());
            }
        }

        public void Add(string text)
        {
            //		$"{nameof(Add)}: {text}".Dump();

            var prev = string.Empty;
            var root2 = _root;
            int km1 = _k - 1;

            int i = 0;
            foreach (var k in Tokenize(text))
            {
                //			$"{nameof(Add)}: [{i}]: {k}".Dump();

                if (0 < i)
                {
                    if (!root2.TryGetValue(prev, out var node))
                    {
                        root2.Add(prev, node = new Node { _nodes = new Dictionary<string, int>() });
                    }

                    var k2 = k.Substring(km1);

                    var nodes = node._nodes;

                    if (!nodes.TryGetValue(k2, out var node2))
                    {
                        nodes.Add(k2, node2 = 0);
                    }

                    nodes[k2] = node2 + 1;

                    prev = k.Substring(1);
                    i++;
                }
                else
                {
                    if (!root2.TryGetValue(string.Empty, out var node))
                    {
                        root2.Add(prev, node = new Node { _nodes = new Dictionary<string, int>() });
                    }

                    var nodes = node._nodes;

                    if (!nodes.TryGetValue(k, out var node2))
                    {
                        nodes.Add(k, node2 = 0);
                    }

                    nodes[k] = node2 + 1;

                    prev = k.Substring(1);
                    i++;
                }
            }
        }

        public void Prepare()
        {
            foreach (var item in _root)
            {
                var node = item.Value;

                node.Prepare();
            }
        }

        private string Find(string prev, Random r)
        {
            var node = _root[prev];

            int p = r.Next(node._total);

            int v = 0;
            foreach (var item in node._nodes)
            {
                var weight = item.Value;

                if ((v <= p) & p < v + weight)
                {
                    return item.Key;
                }

                v += weight;
            }

            throw new InvalidOperationException();
        }

        public string Generate(Random r)
        {
            var sb = new StringBuilder();
            Generate(r, sb);
            return sb.ToString();
        }

        public void Generate(Random r, StringBuilder sb)
        {
            int km1 = _k - 1;

            var init = Find(string.Empty, r);

            //		$"{nameof(Generate)}: Find({string.Empty}) => {init}, IsTerminal={0 < init.Length && init[init.Length - 1] == '\0'}".Dump();

            if (init[init.Length - 1] == '\0')
            {
                sb.Append(init, 0, init.Length - 1);
                return;
            }
            else
            {
                sb.Append(init);
            }

            string prev = init.Substring(1, km1);
            for (int i = 0; ; i++)
            {
                var s = Find(prev, r);

                //			$"{nameof(Generate)}: Find({prev}) => {s}, IsTerminal={0 < s.Length && s[s.Length - 1] == '\0'}".Dump();

                if (s[s.Length - 1] == '\0')
                {
                    break;
                }

                sb.Append(s);

                prev = prev.Substring(1) + s;
            }
        }

        public void WriteToFile(string fn)
        {
            using (var outputStream = File.Create(fn))
            {
                WriteToStream(outputStream);
            }
        }

        public void WriteToStream(Stream outputStream)
        {
            var scratch = new byte[4 * _k];

            outputStream.WriteVarInt63(_k);
            outputStream.WriteVarInt63(_root.Count);

            foreach (var item in _root)
            {
                var node = item.Value;

                var k = Encoding.UTF8.GetBytes(item.Key, 0, item.Key.Length, scratch, 0);

                outputStream.WriteVarInt63(k);
                outputStream.Write(scratch, 0, k);

                node.WriteToStream(outputStream, scratch);
            }
        }

        public void ReadFromFile(string fn)
        {
            using (var inputStream = File.OpenRead(fn))
            {
                ReadFromStream(inputStream);
            }
        }

        public void ReadFromStream(Stream inputStream)
        {
            if (_k != 0) throw new InvalidOperationException();

            var k = (int)inputStream.ReadVarInt63();

            var scratch = new byte[4 * k];

            var count = (int)inputStream.ReadVarInt63();

            var root = new Dictionary<string, Node>(count);

            for (int i = 0; i < count; i++)
            {
                var node = new Node();
                var keyLength = (int)inputStream.ReadVarInt63();
                inputStream.Read(scratch, 0, keyLength);
                var key = Encoding.UTF8.GetString(scratch, 0, keyLength);
                node.ReadFromStream(inputStream, scratch);
                root.Add(key, node);
            }

            _root = root;
            _k = k;
        }
    }
}
