using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotMaysWind.Office.entity;
using DotMaysWind.Office.helper;

namespace DotMaysWind.Office
{
    public sealed class WordBinaryFile : CompoundBinaryFile, IWordFile
    {
        #region 字段
        private ushort _nFib;
        private bool _isComplexFile;
        private bool _hasPictures;
        private bool _isEncrypted;
        private bool _is1Table;

        private ushort _lidFE;

        private int _cbMac;
        private int _ccpText;
        private int _ccpFtn;
        private int _ccpHdd;
        private int _ccpAtn;
        private int _ccpEdn;
        private int _ccpTxbx;
        private int _ccpHdrTxbx;

        private uint _fcClx;
        private uint _lcbClx;

        private List<uint> _lstPieceStartPosition;
        private List<uint> _lstPieceEndPosition;
        private List<PieceElement> _lstPieceElement;

        private string _paragraphText;
        private string _footnoteText;
        private string _headerText;
        private string _commentText;
        private string _endnoteText;
        private string _textboxText;
        private string _headerTextboxText;
        #endregion

        #region 属性
        /// <summary>
        /// 获取应用程序版本
        /// </summary>
        public string Version
        {
            get
            {
                switch (_nFib)
                {
                    case 0x00C1: return "Word 97";
                    case 0x00D9: return "Word 2000";
                    case 0x0101: return "Word 2002";
                    case 0x010C: return "Word 2003";
                    case 0x0112: return "Word 2007";
                    default: return string.Empty;
                }
            }
        }

        /// <summary>
        /// 获取文档正文内容
        /// </summary>
        public string ParagraphText
        {
            get { return _paragraphText; }
        }

        /// <summary>
        /// 获取文档页眉和页脚内容
        /// </summary>
        public string HeaderAndFooterText
        {
            get { return _headerText; }
        }

        /// <summary>
        /// 获取文档批注内容
        /// </summary>
        public string CommentText
        {
            get { return _commentText; }
        }

        /// <summary>
        /// 获取文档脚注内容
        /// </summary>
        public string FootnoteText
        {
            get { return _footnoteText; }
        }

        /// <summary>
        /// 获取文档尾注内容
        /// </summary>
        public string EndnoteText
        {
            get { return _endnoteText; }
        }

        /// <summary>
        /// 获取文档文本框内容
        /// </summary>
        public string TextboxText
        {
            get { return _textboxText; }
        }

        /// <summary>
        /// 获取文档页眉文本框内容
        /// </summary>
        public string HeaderTextboxText
        {
            get { return _headerTextboxText; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化DocFile
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public WordBinaryFile(string filePath) :
            base(filePath) { }
        #endregion

        #region 读取内容
        protected override void ReadContent()
        {
            ReadWordDocument();
            ReadTableStream();
            ReadPieceText();
        }
        #endregion

        #region 读取WordDocument
        private void ReadWordDocument()
        {
            DirectoryEntry entry = _dirRootEntry.GetChild("WordDocument");

            if (entry == null)
            {
                return;
            }

            LoadEntry(entry, new Action<Stream, BinaryReader>((stream, reader) =>
            {
                ReadFileInformationBlock(stream, reader);
            }));
        }

        #region 读取FileInformationBlock
        private void ReadFileInformationBlock(Stream stream, BinaryReader reader)
        {
            ReadFibBase(stream, reader);
            ReadFibRgW97(stream, reader);
            ReadFibRgLw97(stream, reader);
            ReadFibRgFcLcb(stream, reader);
            ReadFibRgCswNew(stream, reader);
        }

        #region FibBase
        private void ReadFibBase(Stream stream, BinaryReader reader)
        {
            ushort wIdent = reader.ReadUInt16();
            if (wIdent != 0xA5EC)
            {
                throw new Exception("This file is not \".doc\" file.");
            }

            _nFib = reader.ReadUInt16();
            reader.ReadUInt16();//unused
            reader.ReadUInt16();//lid
            reader.ReadUInt16();//pnNext

            ushort flags = reader.ReadUInt16();
            _isComplexFile = BitHelper.GetBitFromInteger(flags, 2);
            _hasPictures = BitHelper.GetBitFromInteger(flags, 3);
            _isEncrypted = BitHelper.GetBitFromInteger(flags, 8);
            _is1Table = BitHelper.GetBitFromInteger(flags, 9);

            if (_isComplexFile)
            {
                throw new Exception("Do not support the complex file.");
            }

            if (_isEncrypted)
            {
                throw new Exception("Do not support the encvypted file.");
            }

            stream.Seek(32 - 12, SeekOrigin.Current);
        }
        #endregion

        #region FibRgW97
        private void ReadFibRgW97(Stream stream, BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();

            if (count != 0x000E)
            {
                throw new Exception("File has been broken (FibRgW97 length is INVALID).");
            }

            stream.Seek(26, SeekOrigin.Current);
            _lidFE = reader.ReadUInt16();
        }
        #endregion

        #region FibRgLw97
        private void ReadFibRgLw97(Stream stream, BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();

            if (count != 0x0016)
            {
                throw new Exception("File has been broken (FibRgLw97 length is INVALID).");
            }

            _cbMac = reader.ReadInt32();
            reader.ReadInt32();//reserved1
            reader.ReadInt32();//reserved2
            _ccpText = reader.ReadInt32();
            _ccpFtn = reader.ReadInt32();
            _ccpHdd = reader.ReadInt32();
            reader.ReadInt32();//reserved3
            _ccpAtn = reader.ReadInt32();
            _ccpEdn = reader.ReadInt32();
            _ccpTxbx = reader.ReadInt32();
            _ccpHdrTxbx = reader.ReadInt32();

            stream.Seek(44, SeekOrigin.Current);
        }
        #endregion

        #region FibRgFcLcb
        private void ReadFibRgFcLcb(Stream stream, BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            stream.Seek(66 * 4, SeekOrigin.Current);

            _fcClx = reader.ReadUInt32();
            _lcbClx = reader.ReadUInt32();

            stream.Seek((count * 2 - 68) * 4, SeekOrigin.Current);
        }
        #endregion

        #region FibRgCswNew
        private void ReadFibRgCswNew(Stream stream, BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            _nFib = reader.ReadUInt16();
            stream.Seek((count - 1) * 2, SeekOrigin.Current);
        }
        #endregion
        #endregion
        #endregion

        #region 读取TableStream
        private void ReadTableStream()
        {
            DirectoryEntry entry = _dirRootEntry.GetChild(_is1Table ? "1Table" : "0Table");

            if (entry == null)
            {
                return;
            }

            LoadEntry(entry, new Action<Stream, BinaryReader>((stream, reader) =>
            {
                long pieceTableStart = _fcClx;
                long pieceTableEnd = pieceTableStart + _lcbClx;
                stream.Seek(pieceTableStart, SeekOrigin.Begin);

                byte clxt = reader.ReadByte();
                int prcLen = 0;

                //判断如果是Prc不是Pcdt
                while (clxt == 0x01 && stream.Position < pieceTableEnd)
                {
                    stream.Seek(prcLen, SeekOrigin.Current);
                    clxt = reader.ReadByte();
                    prcLen = reader.ReadInt32();
                }

                if (clxt != 0x02)
                {
                    throw new Exception("There's no content in this file.");
                }

                uint size = reader.ReadUInt32();
                uint count = (size - 4) / 12;

                _lstPieceStartPosition = new List<uint>();
                _lstPieceEndPosition = new List<uint>();
                _lstPieceElement = new List<PieceElement>();

                for (int i = 0; i < count; i++)
                {
                    _lstPieceStartPosition.Add(reader.ReadUInt32());
                    _lstPieceEndPosition.Add(reader.ReadUInt32());
                    stream.Seek(-4, SeekOrigin.Current);
                }

                stream.Seek(4, SeekOrigin.Current);

                for (int i = 0; i < count; i++)
                {
                    ushort info = reader.ReadUInt16();
                    uint fcCompressed = reader.ReadUInt32();
                    ushort prm = reader.ReadUInt16();

                    _lstPieceElement.Add(new PieceElement(info, fcCompressed, prm));
                }
            }));
        }
        #endregion

        #region 读取文本内容
        private void ReadPieceText()
        {
            StringBuilder sb = new StringBuilder();
            DirectoryEntry entry = _dirRootEntry.GetChild("WordDocument");

            LoadEntry(entry, new Action<Stream, BinaryReader>((stream, reader) =>
            {
                for (int i = 0; i < _lstPieceElement.Count; i++)
                {
                    long pieceStart = _lstPieceElement[i].Offset;
                    stream.Seek(pieceStart, SeekOrigin.Begin);

                    int length = (int)((_lstPieceElement[i].IsUnicode ? 2 : 1) * (_lstPieceEndPosition[i] - _lstPieceStartPosition[i]));
                    byte[] data = reader.ReadBytes(length);
                    string content = StringHelper.GetString(_lstPieceElement[i].IsUnicode, data);
                    sb.Append(content);
                }

                string allContent = sb.ToString();
                int paragraphEnd = _ccpText;
                int footnoteEnd = paragraphEnd + _ccpFtn;
                int headerEnd = footnoteEnd + _ccpHdd;
                int commentEnd = headerEnd + _ccpAtn;
                int endnoteEnd = commentEnd + _ccpEdn;
                int textboxEnd = endnoteEnd + _ccpTxbx;
                int headerTextboxEnd = textboxEnd + _ccpHdrTxbx;

                _paragraphText = StringHelper.ReplaceString(allContent.Substring(0, _ccpText));
                _footnoteText = StringHelper.ReplaceString(allContent.Substring(paragraphEnd, _ccpFtn));
                _headerText = StringHelper.ReplaceString(allContent.Substring(footnoteEnd, _ccpHdd));
                _commentText = StringHelper.ReplaceString(allContent.Substring(headerEnd, _ccpAtn));
                _endnoteText = StringHelper.ReplaceString(allContent.Substring(commentEnd, _ccpEdn));
                _textboxText = StringHelper.ReplaceString(allContent.Substring(endnoteEnd, _ccpTxbx));
                _headerTextboxText = StringHelper.ReplaceString(allContent.Substring(textboxEnd, _ccpHdrTxbx));
            }));
        }
        #endregion
    }
}