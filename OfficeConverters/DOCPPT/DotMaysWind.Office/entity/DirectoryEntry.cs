using System;
using System.Collections.Generic;

namespace DotMaysWind.Office.entity
{
    public enum DirectoryEntryType : byte
    {
        Invalid = 0,
        Storage = 1,
        Stream = 2,
        Root = 5
    }

    public class DirectoryEntry
    {
        #region 字段
        private uint _entryID;
        private string _entryName;
        private DirectoryEntryType _entryType;
        private uint _firstSectorID;
        private uint _length;

        private DirectoryEntry _parent;
        private List<DirectoryEntry> _children;
        #endregion

        #region 属性
        /// <summary>
        /// 获取DirectoryEntry的EntryID
        /// </summary>
        public uint EntryID
        {
            get { return _entryID; }
        }

        /// <summary>
        /// 获取DirectoryEntry名称
        /// </summary>
        public string EntryName
        {
            get { return _entryName; }
        }

        /// <summary>
        /// 获取DirectoryEntry类型
        /// </summary>
        public DirectoryEntryType EntryType
        {
            get { return _entryType; }
        }

        /// <summary>
        /// 获取DirectoryEntry的第一个SectorID
        /// </summary>
        public uint FirstSectorID
        {
            get { return _firstSectorID; }
        }

        /// <summary>
        /// 获取DirectoryEntry的内容大小
        /// </summary>
        public uint Length
        {
            get { return _length; }
        }

        /// <summary>
        /// 获取DirectoryEntry的父节点
        /// </summary>
        public DirectoryEntry Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// 获取DirectoryEntry的子节点
        /// </summary>
        public List<DirectoryEntry> Children
        {
            get { return _children; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化新的DirectoryEntry
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="entryID">DirectoryEntryID</param>
        /// <param name="entryName">DirectoryEntry名称</param>
        /// <param name="entryType">DirectoryEntry类型</param>
        /// <param name="firstSectorID">第一个SectorID</param>
        /// <param name="length">内容大小</param>
        public DirectoryEntry(DirectoryEntry parent, uint entryID, string entryName, DirectoryEntryType entryType, uint firstSectorID, uint length)
        {
            _entryID = entryID;
            _entryName = entryName;
            _entryType = entryType;
            _firstSectorID = firstSectorID;
            _length = length;
            _parent = parent;

            if (entryType == DirectoryEntryType.Root || entryType == DirectoryEntryType.Storage)
            {
                _children = new List<DirectoryEntry>();
            }
        }
        #endregion

        #region 方法
        public void AddChild(DirectoryEntry entry)
        {
            if (_children == null)
            {
                _children = new List<DirectoryEntry>();
            }

            _children.Add(entry);
        }

        public DirectoryEntry GetChild(string entryName)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (string.Equals(_children[i].EntryName, entryName))
                {
                    return _children[i];
                }
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", base.ToString(), _entryName);
        }
        #endregion
    }
}