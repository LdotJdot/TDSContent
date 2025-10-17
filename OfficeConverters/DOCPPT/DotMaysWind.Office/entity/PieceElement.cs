using System;

namespace DotMaysWind.Office.entity
{
    public class PieceElement
    {
        #region 字段
        private ushort _info;
        private uint _fc;
        private ushort _prm;
        private bool _isUnicode;
        #endregion

        #region 属性
        /// <summary>
        /// 获取是否以Unicode形式存储文本
        /// </summary>
        public bool IsUnicode
        {
            get { return _isUnicode; }
        }

        /// <summary>
        /// 获取文本偏移量
        /// </summary>
        public uint Offset
        {
            get { return _fc; }
        }
        #endregion

        #region 构造函数
        public PieceElement(ushort info, uint fcCompressed, ushort prm)
        {
            _info = info;
            _fc = fcCompressed & 0x3FFFFFFF;//后30位
            _prm = prm;
            _isUnicode = (fcCompressed & 0x40000000) == 0;//第31位

            if (!_isUnicode) _fc = _fc / 2;
        }
        #endregion
    }
}