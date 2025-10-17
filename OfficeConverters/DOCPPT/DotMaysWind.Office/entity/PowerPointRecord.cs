using System;
using System.Collections.Generic;

namespace DotMaysWind.Office.entity
{
    public enum RecordType : uint
    {
        Unknown = 0,
        DocumentContainer = 0x03E8,
        ListWithTextContainer = 0x0FF0,
        PersistAtom = 0x03F3,
        TextHeaderAtom = 0x0F9F,
        EndDocumentAtom = 0x03EA,
        MainMasterContainer = 0x03F8,
        DrawingContainer = 0x040C,
        SlideContainer = 0x03EE,
        HeadersFootersContainer = 0x0FD9,
        SlideAtom = 0x03EF,
        NotesContainer = 0x03F0,
        TextCharsAtom = 0x0FA0,
        TextBytesAtom = 0x0FA8,
        CString = 0x0FBA
    }

    public class PowerPointRecord
    {
        #region 常量
        private const uint ContainerRecordVersion = 0xF;
        #endregion

        #region 字段
        private ushort _recVer;
        private ushort _recInstance;
        private RecordType _recType;
        private uint _recLen;
        private long _offset;

        private int _deepth;
        private PowerPointRecord _parent;
        private List<PowerPointRecord> _children;
        #endregion

        #region 属性
        /// <summary>
        /// 获取RecordVersion
        /// </summary>
        public ushort RecordVersion
        {
            get { return _recVer; }
        }

        /// <summary>
        /// 获取RecordInstance
        /// </summary>
        public ushort RecordInstance
        {
            get { return _recInstance; }
        }

        /// <summary>
        /// 获取Record类型
        /// </summary>
        public RecordType RecordType
        {
            get { return _recType; }
        }

        /// <summary>
        /// 获取Record内容大小
        /// </summary>
        public uint RecordLength
        {
            get { return _recLen; }
        }
        
        /// <summary>
        /// 获取Record相对PowerPoint Document偏移
        /// </summary>
        public long Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// 获取Record深度
        /// </summary>
        public int Deepth
        {
            get { return _deepth; }
        }

        /// <summary>
        /// 获取Record的父节点
        /// </summary>
        public PowerPointRecord Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// 获取Record的子节点
        /// </summary>
        public List<PowerPointRecord> Children
        {
            get { return _children; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化新的Record
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="version">RecordVersion和Instance</param>
        /// <param name="type">Record类型</param>
        /// <param name="length">Record内容大小</param>
        /// <param name="offset">Record相对PowerPoint Document偏移</param>
        public PowerPointRecord(PowerPointRecord parent, ushort version, ushort type, uint length, long offset)
        {
            _recVer = (ushort)(version & 0xF);
            _recInstance = (ushort)((version & 0xFFF0) >> 4);
            _recType = (RecordType)type;
            _recLen = length;
            _offset = offset;
            _deepth = parent == null ? 0 : parent._deepth + 1;
            _parent = parent;

            if (_recVer == ContainerRecordVersion)
            {
                _children = new List<PowerPointRecord>();
            }
        }
        #endregion

        #region 方法
        public void AddChild(PowerPointRecord entry)
        {
            if (_children == null)
            {
                _children = new List<PowerPointRecord>();
            }

            _children.Add(entry);
        }
        #endregion
    }
}