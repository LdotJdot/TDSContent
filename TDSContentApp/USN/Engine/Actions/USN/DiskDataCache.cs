using EngineCore.Engine.Actions.USN;
using K4os.Compression.LZ4.Streams;
using System.Text;
using TDSNET.Engine.Actions.USN;


namespace TDSContentApp.USN
{

    public class DiskDataCache
    {
        const ulong ENDTAG = ulong.MinValue;
        const string FINALENDTAG = "#FINALEND";

        static public void Discard(string path)
        {
            if (File.Exists(path)) { File.Delete(path); }
        }

        static public void DumpToDisk(List<FileSys2> fileSys,string path, bool dumpFrnDetails)
        {
            Discard(path);
            
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);
            using var lz4s = LZ4Stream.Encode(fileStream, K4os.Compression.LZ4.LZ4Level.L00_FAST);
            //   using GZipStream gz = new GZipStream(fs, CompressionLevel.Fastest);

            using var writer = new BinaryWriter(lz4s, Encoding.UTF8);
            foreach (FileSys2 file in fileSys)
            {
                DumpToDisk(file, writer, dumpFrnDetails);
            }
            writer.Write(FINALENDTAG);
            writer.Flush();
            writer.Close();
            lz4s.Close();
            fileStream.Close();
        }


        static private void DumpToDisk(FileSys2 fileSys, BinaryWriter writer, bool dumpFrnDetails)
        {
            writer.Write(fileSys.DriveName);
            writer.Write(fileSys.DriveFormat);
            writer.Write(fileSys.usnStates.UsnJournalID);
            writer.Write(fileSys.usnStates.FirstUsn);
            writer.Write(fileSys.usnStates.NextUsn);
            writer.Write(fileSys.usnStates.LowestValidUsn);
            writer.Write(fileSys.usnStates.MaxUsn);
            writer.Write(fileSys.usnStates.MaximumSize);
            writer.Write(fileSys.usnStates.AllocationDelta);

            if (dumpFrnDetails)
            {
                foreach (var file in fileSys.files.Values)
                {
                    writer.Write(file.referenceNumber);
                    writer.Write(file.parentReferenceNumber);
                    writer.Write(file.fileName);
                }
            }
            writer.Write(ENDTAG);
        }


        static public List<FileSys2>? TryLoadFromDisk(string path)
        {
            if (!File.Exists(path)) return null;
            else return LoadFromDiskSync(path);
        }

        static private List<FileSys2>? LoadFromDiskSync(string path)
        {
            if (!File.Exists(path))return null;

            try
            {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
                using var lz4s = LZ4Stream.Decode(fileStream);

                // using GZipStream gz = new GZipStream(files, CompressionMode.Decompress, false);

                using var reader = new BinaryReader(lz4s, Encoding.UTF8);

                var fileSys = new List<FileSys2>();
start:;
                var firstLine = reader.ReadString();
                if (firstLine == FINALENDTAG)
                {
                    reader.Close();
                    return fileSys;
                }
                var driverName = firstLine;
                var driveFormat = reader.ReadString();
                var fs = new FileSys2(driverName,driveFormat);

                fs.lastUsnStates.UsnJournalID = reader.ReadUInt64();
                fs.lastUsnStates.FirstUsn = reader.ReadInt64();
                fs.lastUsnStates.NextUsn = reader.ReadInt64();
                fs.lastUsnStates.LowestValidUsn = reader.ReadInt64();
                fs.lastUsnStates.MaxUsn = reader.ReadInt64();
                fs.lastUsnStates.MaximumSize = reader.ReadUInt64();
                fs.lastUsnStates.AllocationDelta = reader.ReadUInt64();
                fs.usnStates = fs.lastUsnStates;
                while (true)
                {                    
                    var nextId = reader.ReadUInt64();
                    if (nextId == ENDTAG)
                    {
                        fileSys.Add(fs);

                        goto start;
                    }
                    else
                    {
                        var refNum = nextId;
                        var parentRefNum = reader.ReadUInt64();
                        var filename = reader.ReadString();

                        fs.files.TryAdd(refNum,new FileEntry(refNum,parentRefNum,filename));
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }





}
