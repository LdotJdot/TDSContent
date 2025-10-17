using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotMaysWind.Office.entity;
using DotMaysWind.Office.summary;

namespace DotMaysWind.Office
{
    public class CompoundBinaryFile : IOfficeFile
    {
        #region 常量
        private const uint HeaderSize = 0x200;//512字节
        private const uint DirectoryEntrySize = 0x80;//128字节
        protected const uint MaxRegSector = 0xFFFFFFFA;
        protected const uint DifSector = 0xFFFFFFFC;
        protected const uint FatSector = 0xFFFFFFFD;
        protected const uint EndOfChain = 0xFFFFFFFE;
        protected const uint FreeSector = 0xFFFFFFFF;
        #endregion

        #region 字段
        protected FileStream _stream;
        protected BinaryReader _reader;
        protected long _length;
        protected List<uint> _fatSectors;
        protected List<uint> _minifatSectors;
        protected List<uint> _miniSectors;
        protected List<uint> _dirSectors;
        protected DirectoryEntry _dirRootEntry;
        protected Dictionary<uint, List<uint>> _entrySectorIDs;
        protected Dictionary<uint, byte[]> _entryData;

        protected List<DocumentSummaryInformation> _documentSummaryInformation;
        protected List<SummaryInformation> _summaryInformation;

        #region 头部信息
        private uint _sectorSize;//Sector大小
        private uint _miniSectorSize;//Mini-Sector大小
        private uint _fatCount;//FAT数量
        private uint _dirStartSectorID;//Directory开始的SectorID
        private uint _miniCutoffSize;//Mini-Sector最大的大小
        private uint _miniFatStartSectorID;//Mini-FAT开始的SectorID
        private uint _miniFatCount;//Mini-FAT数量
        private uint _difStartSectorID;//DIF开始的SectorID
        private uint _difCount;//DIF数量
        #endregion
        #endregion

        #region 属性
        /// <summary>
        /// 获取DocumentSummaryInformation
        /// </summary>
        public Dictionary<string, string> DocumentSummaryInformation
        {
            get
            {
                if (_documentSummaryInformation == null)
                {
                    return null;
                }

                Dictionary<string, string> dict = new Dictionary<string, string>();
                for (int i = 0; i < _documentSummaryInformation.Count; i++)
                {
                    dict.Add(_documentSummaryInformation[i].Type.ToString(), _documentSummaryInformation[i].Data.ToString());
                }

                return dict;
            }
        }

        /// <summary>
        /// 获取SummaryInformation
        /// </summary>
        public Dictionary<string, string> SummaryInformation
        {
            get
            {
                if (_summaryInformation == null)
                {
                    return null;
                }

                Dictionary<string, string> dict = new Dictionary<string, string>();

                for (int i = 0; i < _summaryInformation.Count; i++)
                {
                    dict.Add(_summaryInformation[i].Type.ToString(), _summaryInformation[i].Data.ToString());
                }

                return dict;
            }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化CompoundBinaryFile
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public CompoundBinaryFile(string filePath)
        {
            try
            {
                _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                _reader = new BinaryReader(_stream);

                _length = _stream.Length;

                ReadHeader();
                ReadFAT();
                ReadDirectory();
                ReadMiniFAT();
                ReadEntryData();

                ReadDocumentSummaryInformation();
                ReadSummaryInformation();
                ReadContent();
            }
            finally
            {
                if (_reader != null)
                {
                    _reader.Close();
                }

                if (_stream != null)
                {
                    _stream.Close();
                }
            }
        }
        #endregion

        #region 读取头部信息
        private void ReadHeader()
        {
            if (_reader == null)
            {
                return;
            }

            //先判断是否是Compound Binary文件格式
            byte[] sig = _length > 512 ? _reader.ReadBytes(8) : null;
            if (sig == null ||
                sig[0] != 0xD0 || sig[1] != 0xCF || sig[2] != 0x11 || sig[3] != 0xE0 ||
                sig[4] != 0xA1 || sig[5] != 0xB1 || sig[6] != 0x1A || sig[7] != 0xE1)
            {
                throw new Exception("This file is not compound binary file.");
            }

            //读取头部信息
            _stream.Seek(22, SeekOrigin.Current);
            _sectorSize = (uint)Math.Pow(2, _reader.ReadUInt16());
            _miniSectorSize = (uint)Math.Pow(2, _reader.ReadUInt16());

            _stream.Seek(10, SeekOrigin.Current);
            _fatCount = _reader.ReadUInt32();
            _dirStartSectorID = _reader.ReadUInt32();

            _stream.Seek(4, SeekOrigin.Current);
            _miniCutoffSize = _reader.ReadUInt32();
            _miniFatStartSectorID = _reader.ReadUInt32();
            _miniFatCount = _reader.ReadUInt32();
            _difStartSectorID = _reader.ReadUInt32();
            _difCount = _reader.ReadUInt32();
        }
        #endregion

        #region 读取FAT
        private void ReadFAT()
        {
            if (_fatCount > 0)
            {
                _fatSectors = new List<uint>();
                ReadFirst109FatSectors();
            }

            if (_difCount > 0)
            {
                ReadLastFatSectors();
            }

            if (_fatCount != _fatSectors.Count)
            {
                throw new Exception("File has been broken (FAT count is INVALID).");
            }
        }

        private void ReadFirst109FatSectors()
        {
            for (int i = 0; i < 109; i++)
            {
                uint nextSector = _reader.ReadUInt32();

                if (nextSector == FreeSector)
                {
                    break;
                }

                _fatSectors.Add(nextSector);
            }
        }

        private void ReadLastFatSectors()
        {
            uint difSectorID = _difStartSectorID;

            while (true)
            {
                long entryStart = GetNormalSectorOffset(difSectorID);
                _stream.Seek(entryStart, SeekOrigin.Begin);

                for (int i = 0; i < 127; i++)
                {
                    uint fatSectorID = _reader.ReadUInt32();

                    if (fatSectorID == FreeSector)
                    {
                        return;
                    }

                    _fatSectors.Add(fatSectorID);
                }

                difSectorID = _reader.ReadUInt32();
                if (difSectorID == EndOfChain)
                {
                    break;
                }
            }
        }
        #endregion

        #region 读取目录信息
        private void ReadDirectory()
        {
            if (_reader == null)
            {
                return;
            }

            _dirSectors = new List<uint>();

            uint sectorID = _dirStartSectorID;

            while (true)
            {
                _dirSectors.Add(sectorID);
                sectorID = GetNextNormalSectorID(sectorID);

                if (sectorID == EndOfChain)
                {
                    break;
                }
            }

            uint leftSiblingEntryID, rightSiblingEntryID, childEntryID;
            _dirRootEntry = GetDirectoryEntry(0, null, out leftSiblingEntryID, out rightSiblingEntryID, out childEntryID);
            ReadDirectoryEntry(_dirRootEntry, childEntryID);
        }

        private void ReadDirectoryEntry(DirectoryEntry rootEntry, uint entryID)
        {
            uint leftSiblingEntryID, rightSiblingEntryID, childEntryID;
            DirectoryEntry entry = GetDirectoryEntry(entryID, rootEntry, out leftSiblingEntryID, out rightSiblingEntryID, out childEntryID);

            if (entry == null || entry.EntryType == DirectoryEntryType.Invalid)
            {
                return;
            }
            
            rootEntry.AddChild(entry);

            if (leftSiblingEntryID < uint.MaxValue)//有左兄弟节点
            {
                ReadDirectoryEntry(rootEntry, leftSiblingEntryID);
            }

            if (rightSiblingEntryID < uint.MaxValue)//有右兄弟节点
            {
                ReadDirectoryEntry(rootEntry, rightSiblingEntryID);
            }

            if (childEntryID < uint.MaxValue)//有孩子节点
            {
                ReadDirectoryEntry(entry, childEntryID);
            }
        }

        private DirectoryEntry GetDirectoryEntry(uint entryID, DirectoryEntry parentEntry, out uint leftSiblingEntryID, out uint rightSiblingEntryID, out uint childEntryID)
        {
            leftSiblingEntryID = ushort.MaxValue;
            rightSiblingEntryID = ushort.MaxValue;
            childEntryID = ushort.MaxValue;

            _stream.Seek(GetDirectoryEntryOffset(entryID), SeekOrigin.Begin);

            if (_stream.Position >= _length)
            {
                return null;
            }

            StringBuilder temp = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                temp.Append((char)_reader.ReadUInt16());
            }

            ushort nameLen = _reader.ReadUInt16();
            string name = temp.ToString(0, temp.Length < nameLen / 2 - 1 ? temp.Length : nameLen / 2 - 1);
            byte type = _reader.ReadByte();

            if (type > 5)
            {
                return null;
            }

            _stream.Seek(1, SeekOrigin.Current);
            leftSiblingEntryID = _reader.ReadUInt32();
            rightSiblingEntryID = _reader.ReadUInt32();
            childEntryID = _reader.ReadUInt32();

            _stream.Seek(36, SeekOrigin.Current);
            uint firstSectorID = _reader.ReadUInt32();
            uint length = _reader.ReadUInt32();

            return new DirectoryEntry(parentEntry, entryID, name, (DirectoryEntryType)type, firstSectorID, length);
        }
        #endregion

        #region 读取MiniFAT
        private void ReadMiniFAT()
        {
            if (_miniFatCount > 0)
            {
                _minifatSectors = new List<uint>();
                ReadMiniFatSectors();
            }

            if (_minifatSectors != null && _minifatSectors.Count > 0)
            {
                _miniSectors = new List<uint>();
                ReadMiniSectors();
            }

            if (_miniSectors != null && _miniSectors.Count != Math.Ceiling((double)_dirRootEntry.Length / _sectorSize))
            {
                throw new Exception("File has been broken (mini-FAT count is INVALID)");
            }
        }

        private void ReadMiniFatSectors()
        {
            uint sectorID = _miniFatStartSectorID;

            while (true)
            {
                _minifatSectors.Add(sectorID);
                sectorID = GetNextNormalSectorID(sectorID);

                if (sectorID == EndOfChain)
                {
                    break;
                }
            }
        }

        private void ReadMiniSectors()
        {
            uint sectorID = _dirRootEntry.FirstSectorID;

            while (true)
            {
                _miniSectors.Add(sectorID);
                sectorID = GetNextNormalSectorID(sectorID);

                if (sectorID == EndOfChain)
                {
                    break;
                }
            }
        }
        #endregion

        #region 读取Entry内容
        private void ReadEntryData()
        {
            if (_reader == null)
            {
                return;
            }

            _entrySectorIDs = new Dictionary<uint, List<uint>>();
            _entryData = new Dictionary<uint, byte[]>();

            for (int i = 0; i < _dirRootEntry.Children.Count; i++)
            {
                DirectoryEntry entry = _dirRootEntry.Children[i];

                ReadEntryOrder(entry);
                CopyEntryData(entry);
            }
        }

        private void ReadEntryOrder(DirectoryEntry entry)
        {
            List<uint> sectorIDs = new List<uint>();
            uint sectorID = entry.FirstSectorID;

            while (true)
            {
                if (sectorID == EndOfChain || sectorID == FreeSector)
                {
                    break;
                }
                
                sectorIDs.Add(sectorID);
                sectorID = GetNextSectorID(sectorID, entry.Length >= _miniCutoffSize);
            }

            _entrySectorIDs[entry.EntryID] = sectorIDs;
        }

        private void CopyEntryData(DirectoryEntry entry)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                List<uint> sectorIDs = _entrySectorIDs[entry.EntryID];

                for (int i = 0; i < sectorIDs.Count; i++)
                {
                    long offset = GetSectorOffset(sectorIDs[i], entry.Length >= _miniCutoffSize);
                    uint count = entry.Length >= _miniCutoffSize ? _sectorSize : _miniSectorSize;

                    byte[] buff = new byte[count];
                    _stream.Seek(offset, SeekOrigin.Begin);
                    _stream.Read(buff, 0, (int)count);

                    stream.Write(buff, 0, (int)count);
                }

                _entryData[entry.EntryID] = stream.ToArray();
            }
        }
        #endregion

        #region 读取DocumentSummaryInformation
        private void ReadDocumentSummaryInformation()
        {
            DirectoryEntry entry = _dirRootEntry.GetChild('\x05' + "DocumentSummaryInformation");

            if (entry == null)
            {
                return;
            }

            LoadEntry(entry, new Action<Stream, BinaryReader>((stream, reader) =>
            {
                stream.Seek(24, SeekOrigin.Begin);
                uint propertysCount = reader.ReadUInt32();
                uint docSumamryStart = 0;

                for (int i = 0; i < propertysCount; i++)
                {
                    byte[] clsid = reader.ReadBytes(16);
                    if (clsid.Length == 16 &&
                        clsid[0] == 0x02 && clsid[1] == 0xD5 && clsid[2] == 0xCD && clsid[3] == 0xD5 &&
                        clsid[4] == 0x9C && clsid[5] == 0x2E && clsid[6] == 0x1B && clsid[7] == 0x10 &&
                        clsid[8] == 0x93 && clsid[9] == 0x97 && clsid[10] == 0x08 && clsid[11] == 0x00 &&
                        clsid[12] == 0x2B && clsid[13] == 0x2C && clsid[14] == 0xF9 && clsid[15] == 0xAE)//如果是DocumentSummaryInformation
                    {
                        docSumamryStart = reader.ReadUInt32();
                        break;
                    }
                    else
                    {
                        //stream.Seek(4, SeekOrigin.Current);
                        return;
                    }
                }

                if (docSumamryStart == 0)
                {
                    return;
                }

                stream.Seek(docSumamryStart, SeekOrigin.Begin);
                _documentSummaryInformation = new List<DocumentSummaryInformation>();
                uint docSummarySize = reader.ReadUInt32();
                uint docSummaryCount = reader.ReadUInt32();
                long offsetMark = stream.Position;
                int codePage = Encoding.Default.CodePage;

                for (int i = 0; i < docSummaryCount; i++)
                {
                    if (offsetMark >= stream.Length)
                    {
                        break;
                    }

                    stream.Seek(offsetMark, SeekOrigin.Begin);
                    uint propertyID = reader.ReadUInt32();
                    uint properyOffset = reader.ReadUInt32();

                    offsetMark = stream.Position;

                    stream.Seek(docSumamryStart + properyOffset, SeekOrigin.Begin);

                    if (stream.Position > stream.Length)
                    {
                        continue;
                    }

                    stream.Seek(docSumamryStart + properyOffset, SeekOrigin.Begin);
                    uint propertyType = reader.ReadUInt32();
                    DocumentSummaryInformation info = null;
                    byte[] data = null;

                    if (propertyType == 0x1E)
                    {
                        uint strLen = reader.ReadUInt32();
                        data = reader.ReadBytes((int)strLen);
                        info = new DocumentSummaryInformation(propertyID, propertyType, codePage, data);
                    }
                    else
                    {
                        data = reader.ReadBytes(4);
                        info = new DocumentSummaryInformation(propertyID, propertyType, data);

                        if (info.Type == DocumentSummaryInformationType.CodePage && info.Data != null)//如果找到CodePage的属性
                        {
                            codePage = (ushort)info.Data;
                        }
                    }

                    if (info.Data != null)
                    {
                        _documentSummaryInformation.Add(info);
                    }
                }
            }));
        }
        #endregion

        #region 读取SummaryInformation
        private void ReadSummaryInformation()
        {
            DirectoryEntry entry = _dirRootEntry.GetChild('\x05' + "SummaryInformation");

            if (entry == null)
            {
                return;
            }

            LoadEntry(entry, new Action<Stream, BinaryReader>((stream, reader) =>
            {
                stream.Seek(24, SeekOrigin.Begin);
                uint propertysCount = reader.ReadUInt32();
                uint docSumamryStart = 0;

                for (int i = 0; i < propertysCount; i++)
                {
                    byte[] clsid = reader.ReadBytes(16);
                    if (clsid.Length == 16 &&
                        clsid[0] == 0xE0 && clsid[1] == 0x85 && clsid[2] == 0x9F && clsid[3] == 0xF2 &&
                        clsid[4] == 0xF9 && clsid[5] == 0x4F && clsid[6] == 0x68 && clsid[7] == 0x10 &&
                        clsid[8] == 0xAB && clsid[9] == 0x91 && clsid[10] == 0x08 && clsid[11] == 0x00 &&
                        clsid[12] == 0x2B && clsid[13] == 0x27 && clsid[14] == 0xB3 && clsid[15] == 0xD9)//如果是SummaryInformation
                    {
                        docSumamryStart = reader.ReadUInt32();
                        break;
                    }
                    else
                    {
                        //stream.Seek(4, SeekOrigin.Current);
                        return;
                    }
                }

                if (docSumamryStart == 0)
                {
                    return;
                }

                stream.Seek(docSumamryStart, SeekOrigin.Begin);
                _summaryInformation = new List<SummaryInformation>();
                uint docSummarySize = reader.ReadUInt32();
                uint docSummaryCount = reader.ReadUInt32();
                long offsetMark = stream.Position;
                int codePage = Encoding.Default.CodePage;

                for (int i = 0; i < docSummaryCount; i++)
                {
                    if (offsetMark >= stream.Length)
                    {
                        break;
                    }

                    stream.Seek(offsetMark, SeekOrigin.Begin);
                    uint propertyID = reader.ReadUInt32();
                    uint properyOffset = reader.ReadUInt32();

                    offsetMark = stream.Position;

                    stream.Seek(docSumamryStart + properyOffset, SeekOrigin.Begin);

                    if (stream.Position > stream.Length)
                    {
                        continue;
                    }

                    uint propertyType = reader.ReadUInt32();
                    SummaryInformation info = null;
                    byte[] data = null;

                    if (propertyType == 0x1E)
                    {
                        uint strLen = reader.ReadUInt32();
                        data = reader.ReadBytes((int)strLen);
                        info = new SummaryInformation(propertyID, propertyType, codePage, data);
                    }
                    else if (propertyType == 0x40)
                    {
                        data = reader.ReadBytes(8);
                        info = new SummaryInformation(propertyID, propertyType, data);
                    }
                    else
                    {
                        data = reader.ReadBytes(4);
                        info = new SummaryInformation(propertyID, propertyType, data);

                        if (info.Type == SummaryInformationType.CodePage && info.Data != null)//如果找到CodePage的属性
                        {
                            codePage = (ushort)info.Data;
                        }
                    }

                    if (info.Data != null)
                    {
                        _summaryInformation.Add(info);
                    }
                }
            }));
        }
        #endregion

        #region 读取正文内容
        protected virtual void ReadContent()
        {
            //Do Nothing
        }
        #endregion

        #region 辅助方法
        protected void LoadEntry(DirectoryEntry entry, Action<Stream, BinaryReader> action)
        {
            if (_entryData == null)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(_entryData[entry.EntryID]))
            {
                BinaryReader reader = new BinaryReader(stream);

                action(stream, reader);

                reader.Close();
                reader.Dispose();
            }
        }

        protected long GetSectorOffset(uint sectorID, bool isNormalSector)
        {
            if (isNormalSector)
            {
                return GetNormalSectorOffset(sectorID);
            }
            else
            {
                return GetMiniSectorOffset(sectorID);
            }
        }

        protected uint GetNextSectorID(uint sectorID, bool isNormalSector)
        {
            if (isNormalSector)
            {
                return GetNextNormalSectorID(sectorID);
            }
            else
            {
                return GetNextMiniSectorID(sectorID);
            }
        }

        private long GetNormalSectorOffset(uint sectorID)
        {
            return HeaderSize + _sectorSize * sectorID;
        }

        private long GetMiniSectorOffset(uint miniSectorID)
        {
            uint sectorID = _miniSectors[(int)(miniSectorID * _miniSectorSize / _sectorSize)];
            uint offset = miniSectorID * _miniSectorSize % _sectorSize;

            return HeaderSize + _sectorSize * sectorID + offset;
        }

        private uint GetNextNormalSectorID(uint sectorID)
        {
            uint sectorInFile = _fatSectors[(int)(sectorID / 128)];
            _stream.Seek(GetNormalSectorOffset(sectorInFile) + 4 * (sectorID % 128), SeekOrigin.Begin);

            return _reader.ReadUInt32();
        }

        private uint GetNextMiniSectorID(uint miniSectorID)
        {
            uint sectorInFile = _minifatSectors[(int)(miniSectorID / 128)];
            _stream.Seek(GetNormalSectorOffset(sectorInFile) + 4 * (miniSectorID % 128), SeekOrigin.Begin);

            return _reader.ReadUInt32();
        }

        private long GetDirectoryEntryOffset(uint entryID)
        {
            uint sectorID = _dirSectors[(int)(entryID * DirectoryEntrySize / _sectorSize)];
            return GetNormalSectorOffset(sectorID) + entryID * DirectoryEntrySize % _sectorSize;
        }
        #endregion
    }
}