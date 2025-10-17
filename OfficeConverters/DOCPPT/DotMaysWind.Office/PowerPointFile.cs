using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotMaysWind.Office.entity;
using DotMaysWind.Office.helper;

namespace DotMaysWind.Office
{
    public class PowerPointFile : CompoundBinaryFile, IPowerPointFile
    {
        #region 字段
        private MemoryStream _contentStream;
        private BinaryReader _contentReader;

        private List<PowerPointRecord> _records;
        private StringBuilder _allText;
        #region 测试方法
        private StringBuilder _recordTree;
        #endregion
        #endregion

        #region 属性
        /// <summary>
        /// 获取PowerPoint幻灯片中所有文本
        /// </summary>
        public string AllText
        {
            get { return _allText.ToString(); }
        }

        #region 测试方法
        /// <summary>
        /// 获取PowerPoint中Record的树形结构
        /// </summary>
        public string RecordTree
        {
            get { return _recordTree.ToString(); }
        }
        #endregion
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化PptFile
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public PowerPointFile(string filePath) :
            base(filePath) { }
        #endregion

        #region 读取内容
        protected override void ReadContent()
        {
            DirectoryEntry entry = _dirRootEntry.GetChild("PowerPoint Document");

            if (entry == null)
            {
                return;
            }

            try
            {
                _contentStream = new MemoryStream(_entryData[entry.EntryID]);
                _contentReader = new BinaryReader(_contentStream);

                #region 测试方法
                _recordTree = new StringBuilder();
                #endregion

                _allText = new StringBuilder();
                _records = new List<PowerPointRecord>();
                PowerPointRecord record = null;

                while (_contentStream.Position < _contentStream.Length)
                {
                    record = ReadRecord(null);

                    if (record == null || record.RecordType == 0)
                    {
                        break;
                    }
                }

                _allText = new StringBuilder(StringHelper.ReplaceString(_allText.ToString()));
            }
            finally
            {
                if (_contentReader != null)
                {
                    _contentReader.Close();
                }

                if (_contentStream != null)
                {
                    _contentStream.Close();
                }
            }
        }

        private PowerPointRecord ReadRecord(PowerPointRecord parent)
        {
            PowerPointRecord record = GetRecord(parent);

            if (record == null)
            {
                return null;
            }
            #region 测试方法
            else
            {
                _recordTree.Append('-', record.Deepth * 2);
                _recordTree.AppendFormat("[{0}]-[{1}]-[Len:{2}]", record.RecordType, record.Deepth, record.RecordLength);
                _recordTree.AppendLine();
            }
            #endregion

            if (parent == null)
            {
                _records.Add(record);
            }
            else
            {
                parent.AddChild(record);
            }

            if (record.RecordVersion == 0xF)
            {
                while (_contentStream.Position < record.Offset + record.RecordLength)
                {
                    ReadRecord(record);
                }
            }
            else
            {
                if (record.Parent != null && (
                    record.Parent.RecordType == RecordType.ListWithTextContainer ||
                    record.Parent.RecordType == RecordType.HeadersFootersContainer ||
                    (uint)record.Parent.RecordType == 0xF00D))
                {
                    if (record.RecordType == RecordType.TextCharsAtom || record.RecordType == RecordType.CString)//找到Unicode双字节文字内容
                    {
                        byte[] data = _contentReader.ReadBytes((int)record.RecordLength);
                        _allText.Append(StringHelper.GetString(true, data));
                        _allText.AppendLine();

                    }
                    else if (record.RecordType == RecordType.TextBytesAtom)//找到Unicode<256单字节文字内容
                    {
                        byte[] data = _contentReader.ReadBytes((int)record.RecordLength);
                        _allText.Append(StringHelper.GetString(false, data));
                        _allText.AppendLine();
                    }
                    else
                    {
                        _contentStream.Seek(record.RecordLength, SeekOrigin.Current);
                    }
                }
                else
                {
                    if (_contentStream.Position + record.RecordLength < _contentStream.Length)
                    {
                        _contentStream.Seek(record.RecordLength, SeekOrigin.Current);
                    }
                    else
                    {
                        _contentStream.Seek(0, SeekOrigin.End);
                    }
                }
            }

            return record;
        }

        private PowerPointRecord GetRecord(PowerPointRecord parent)
        {
            if (_contentStream.Position + 8 >= _contentStream.Length)
            {
                return null;
            }

            ushort version = _contentReader.ReadUInt16();
            ushort type = _contentReader.ReadUInt16();
            uint length = _contentReader.ReadUInt32();

            return new PowerPointRecord(parent, version, type, length, _contentStream.Position);
        }
        #endregion
    }
}