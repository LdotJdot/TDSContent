using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Cn.Smart;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace TDSContentCore.Engine
{
    public enum SearchMode
    {
        Pharse,
        TermQuery,
        WildcardQuery,
        FuzzyQuery,
        TermRangeQuery
    }


    public class TDSContentEngine : IDisposable
    {   

        public const string FILEREFERENCENUMBER = "uid";
        public const string DRIVERNAME = "drive";
        public const string FILECONTENT = "content";
        public const string FILEUPDATETIME = "lastupdate";
        private readonly string _indexName;
        private readonly string _indexPath;
        private readonly FSDirectory _directory;
        private readonly IndexWriter _writer;
        private readonly Dictionary<string, IFileToStringConverter> _converters;
        private bool _disposed = false;

        public IEnumerable<string> SupportedFormats => _converters.Keys;


        public void Commit()
        {
            _writer.Commit();
        }

        public TDSContentEngine(string indexName, Dictionary<string, IFileToStringConverter> converters)
        {
            _indexName = indexName ?? throw new ArgumentNullException(nameof(indexName));
            _converters = converters;

            // 创建索引目录
            _indexPath = Path.Combine(indexName);

            // 初始化Lucene目录和写入器
            _directory = FSDirectory.Open(_indexPath);

            // 检查并清除可能存在的旧锁
            if (IndexWriter.IsLocked(_directory))
            {
                IndexWriter.Unlock(_directory);
            }

            var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, LuceneHelper.analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
                RAMBufferSizeMB = 512, // 超大内存缓冲
                MaxBufferedDocs = IndexWriterConfig.DISABLE_AUTO_FLUSH,
                UseCompoundFile = false,  // 关闭复合文件，加快写入
            };

            _writer = new IndexWriter(_directory, config);
        }

        public IList<Lucene.Net.Documents.Document> Search(string keywords, SearchMode mode = SearchMode.Pharse, int maxCount = 100)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TDSContentEngine));
            if (string.IsNullOrWhiteSpace(keywords)) return [];

            try
            {
                using var reader = DirectoryReader.Open(_writer, true);
                return LuceneHelper.Search(reader, keywords, mode, maxCount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Search error: {ex}");
                return [];
            }
        }

        private IFileToStringConverter GetConverter(string ext)
        {
            if (_converters?.TryGetValue(ext, out var converter) == true)
            {
                return converter;
            }
            else if(_converters?.TryGetValue(".txt", out var defaultPainConverter) == true)
            {
                return defaultPainConverter;
            }
            else
            {
                throw new NotSupportedException($"File format not supported and no default converter set: {ext}");
            }
        }

        public void IndexFile(string filePath,ulong uid)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TDSContentEngine));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var extension = Path.GetExtension(filePath);
          
            try
            {
                var converter = GetConverter(extension);
                var content = converter.Convert(filePath);

                LuceneHelper.IndexDocument(_writer, filePath, Path.GetPathRoot(filePath), uid, content);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error indexing file {filePath}: {ex}");
                throw;
            }
        }

        public void DeleteFile(ulong uid)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TDSContentEngine));

            try
            {
                LuceneHelper.DeleteDocument(_writer, uid);
                _writer.Commit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting file {uid}: {ex}");
                throw;
            }
        }

        public IEnumerable<Lucene.Net.Documents.Document> ListAllDocuments()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TDSContentEngine));

            try
            {
                using var reader = DirectoryReader.Open(_writer, true);
                return LuceneHelper.GetAllDocuments(reader);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error listing documents: {ex}");
                return Enumerable.Empty<Lucene.Net.Documents.Document>();
            }
        }

        public long GetDocumentCount()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TDSContentEngine));

            try
            {
                using var reader = DirectoryReader.Open(_writer, true);
                return reader.NumDocs;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting document count: {ex}");
                return 0;
            }
        }

        public void OptimizeIndex()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TDSContentEngine));

            try
            {
                _writer.ForceMerge(1); // 优化索引
                _writer.Commit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error optimizing index: {ex}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    _writer?.Dispose();
                    _directory?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TDSContentEngine()
        {
            Dispose(false);
        }
    }

}