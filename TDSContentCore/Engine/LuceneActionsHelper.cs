using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Cn.Smart;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSContentCore.Engine
{
    public interface IFileToStringConverter : IDisposable
    {
        string Extension { get; }
        string Convert(string filepath);
    }
  
    internal static class LuceneHelper
    {
        public static void IndexDocument(IndexWriter writer, string filePath,string driver, ulong uid, string content)
        {

                var doc = new Lucene.Net.Documents.Document();

                // 添加文件路径字段（存储，不分词）
                doc.Add(new Lucene.Net.Documents.StringField(TDSContentEngine.FILEREFERENCENUMBER, uid.ToString(), Lucene.Net.Documents.Field.Store.YES));
                doc.Add(new Lucene.Net.Documents.StringField(TDSContentEngine.DRIVERNAME, driver, Lucene.Net.Documents.Field.Store.YES));
                doc.Add(new Lucene.Net.Documents.StringField(TDSContentEngine.FILEUPDATETIME, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Lucene.Net.Documents.Field.Store.YES));

                // 添加内容字段（存储，分词）
                doc.Add(new Lucene.Net.Documents.TextField(TDSContentEngine.FILECONTENT, content, Lucene.Net.Documents.Field.Store.YES));

                // 先删除可能存在的旧文档（基于文件路径）
                DeleteDocument(writer, uid);

                // 添加新文档
                writer.AddDocument(doc);
        }

        public static void DeleteDocument(IndexWriter writer, ulong fileReferenceNumber)
        {
            var deleteQuery = new Lucene.Net.Search.TermQuery(new Term(TDSContentEngine.FILEREFERENCENUMBER, fileReferenceNumber.ToString()));
            writer.DeleteDocuments(deleteQuery);
        }

        public static void UpdateDocument(IndexWriter writer, string filePath, string driver,ulong uid, string newContent)
        {
            // 更新文档实际上是先删除再添加
            IndexDocument(writer, filePath,driver, uid, newContent);
        }


        static List<string> GetKeyWords(string q)
        {
            List<string> keyworkds = new List<string>();
            Analyzer analyzer = new SmartChineseAnalyzer(LuceneVersion.LUCENE_48);
            using (var ts = analyzer.GetTokenStream(null, q))
            {
                ts.Reset();
                var ct = ts.GetAttribute<Lucene.Net.Analysis.TokenAttributes.ICharTermAttribute>();

                while (ts.IncrementToken())
                {
                    StringBuilder keyword = new StringBuilder();
                    for (int i = 0; i < ct.Length; i++)
                    {
                        keyword.Append(ct.Buffer[i]);
                    }
                    string item = keyword.ToString();
                    if (!keyworkds.Contains(item))
                    {
                        keyworkds.Add(item);
                    }
                }
            }
            return keyworkds;
        }


        internal static IList<Document> Search(IndexReader reader, string q, SearchMode mode = SearchMode.Pharse, int topn = 200)
        {
            switch (mode)
            {
                case SearchMode.Pharse:
                default:
                    return SearchPharse(reader, q, topn);
                case SearchMode.TermQuery:
                    return SearchTerm(reader, q, topn);
                case SearchMode.WildcardQuery:
                    return SearchWildcardQuery(reader, q, topn);
                case SearchMode.FuzzyQuery:
                    return SearchFuzzy(reader, q, topn);
            }
        }

        static IList<Document> SearchPharse(IndexReader reader, string q, int topn)
        {
            var searcher = new IndexSearcher(reader);

            var keyWordQuery = new PhraseQuery();
            foreach (var item in GetKeyWords(q))
            {
                keyWordQuery.Add(new Term(TDSContentEngine.FILECONTENT, item));
            }
            var hits = searcher.Search(keyWordQuery, topn).ScoreDocs;

            return hits.Select(hit => searcher.Doc(hit.Doc)).ToList();
        }
        static IList<Document> SearchTerm(IndexReader reader, string q, int topn)
        {

            var searcher = new IndexSearcher(reader);

            var keyWordQuery = new BooleanQuery();
            foreach (var item in GetKeyWords(q))
            {
                keyWordQuery.Add(new TermQuery(new Term(TDSContentEngine.FILECONTENT, item)), Occur.MUST);
            }
            var hits = searcher.Search(keyWordQuery, topn).ScoreDocs;

            return hits.Select(hit => searcher.Doc(hit.Doc)).ToList();
        }

        static IList<Document> SearchWildcardQuery(IndexReader reader, string q, int topn)
        {
            var searcher = new IndexSearcher(reader);

            var keyWordQuery = new BooleanQuery();
            foreach (var item in GetKeyWords(q))
            {
                keyWordQuery.Add(new WildcardQuery(new Term(TDSContentEngine.FILECONTENT, item)), Occur.MUST);
            }

            var hits = searcher.Search(keyWordQuery, topn).ScoreDocs;

            return hits.Select(hit => searcher.Doc(hit.Doc)).ToList();
        }
        static IList<Document> SearchFuzzy(IndexReader reader, string q, int topn)
        {
            var searcher = new IndexSearcher(reader);

            var keyWordQuery = new BooleanQuery();
            foreach (var item in GetKeyWords(q))
            {
                keyWordQuery.Add(new FuzzyQuery(new Term(TDSContentEngine.FILECONTENT, item)), Occur.MUST);
            }

            var hits = searcher.Search(keyWordQuery, topn).ScoreDocs;

            return hits.Select(hit => searcher.Doc(hit.Doc)).ToList();
        }


        public static IList<Lucene.Net.Documents.Document> GetAllDocuments(IndexReader reader)
        {
            var results = new List<Lucene.Net.Documents.Document>();

            for (int i = 0; i < reader.MaxDoc; i++)
            {
                // 使用 Document(i, false) 来避免抛出异常，如果文档被删除则返回null
                var doc = reader.Document(i);
                if (doc != null)
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        public static IList<Lucene.Net.Documents.Document> SearchByField(IndexReader reader, string fieldName, string fieldValue)
        {
            if (reader.NumDocs == 0)
                return [];

            var searcher = new Lucene.Net.Search.IndexSearcher(reader);
            var query = new Lucene.Net.Search.TermQuery(new Term(fieldName, fieldValue));

            try
            {
                var hits = searcher.Search(query, 1000).ScoreDocs;
                return hits.Select(hit => searcher.Doc(hit.Doc)).Where(doc => doc != null).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Field search error: {ex}");
                return Enumerable.Empty<Lucene.Net.Documents.Document>().ToList();
            }
        }

        public static void ClearAllDocuments(IndexWriter writer)
        {
            writer.DeleteAll();
            writer.Commit();
        }
    }
}