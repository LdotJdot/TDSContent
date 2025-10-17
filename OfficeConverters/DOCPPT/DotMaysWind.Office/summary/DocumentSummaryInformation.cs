using System;
using System.Text;

namespace DotMaysWind.Office.summary
{
    public class DocumentSummaryInformation
    {
        #region 字段
        private DocumentSummaryInformationType _propertyID;
        private object _data;
        #endregion

        #region 属性
        /// <summary>
        /// 获取属性类型
        /// </summary>
        public DocumentSummaryInformationType Type
        {
            get { return _propertyID; }
        }

        /// <summary>
        /// 获取属性数据
        /// </summary>
        public object Data
        {
            get { return _data; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化新的非字符串型DocumentSummaryInformation
        /// </summary>
        /// <param name="propertyID">属性ID</param>
        /// <param name="propertyType">属性数据类型</param>
        /// <param name="data">属性数据</param>
        public DocumentSummaryInformation(uint propertyID, uint propertyType, byte[] data)
        {
            _propertyID = (DocumentSummaryInformationType)propertyID;
            if (propertyType == 0x02) _data = BitConverter.ToUInt16(data, 0);
            else if (propertyType == 0x03) _data = BitConverter.ToUInt32(data, 0);
            else if (propertyType == 0x0B) _data = BitConverter.ToBoolean(data, 0);
        }

        /// <summary>
        /// 初始化新的字符串型DocumentSummaryInformation
        /// </summary>
        /// <param name="propertyID">属性ID</param>
        /// <param name="propertyType">属性数据类型</param>
        /// <param name="codePage">代码页标识符</param>
        /// <param name="data">属性数据</param>
        public DocumentSummaryInformation(uint propertyID, uint propertyType, int codePage, byte[] data)
        {
            _propertyID = (DocumentSummaryInformationType)propertyID;
            if (propertyType == 0x1E) _data = Encoding.GetEncoding(codePage).GetString(data).Replace("\0", "");
        }
        #endregion
    }
}