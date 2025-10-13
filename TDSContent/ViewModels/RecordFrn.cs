﻿using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using TDSContentCore;

namespace TDSAot.ViewModels
{
    public class RecordFrn : ReactiveObject
    {
        private const int maxFreq = 100;
        private const int initFreq = 50;
        private const int increFreq = 15;
        public IFrnFileOrigin file;
        public int freq = initFreq;

        public RecordFrn(IFrnFileOrigin f)
        {
            this.file = f;
        }

        public RecordFrn(IFrnFileOrigin f, int freq)
        {
            this.file = f;
            this.freq = freq;
        }

        public override string ToString()
        {
            if (file != null)
            {
                return $"{file.FileName}@{file.FilePath[0]}@{freq}";
            }
            else
            {
                return string.Empty;
            }
        }

        public static void Update(ref List<RecordFrn> records, IFrnFileOrigin? newRecord = null)
        {
            if (newRecord != null)
            {
                var oldrec = records.FirstOrDefault(o => o.file == newRecord);
                if (oldrec != null)
                {
                    if (oldrec.freq < initFreq) oldrec.freq = initFreq;

                    oldrec.freq += increFreq;
                }
                else
                {
                    records.Add(new RecordFrn(newRecord));
                }

                records = records.OrderByDescending(o => o.freq).ToList();

                for (int i = 0; i < records.Count; i++)
                {
                    records[i].freq -= i;
                }
            }
            records = records.OrderByDescending(o => o.freq).ToList();
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].freq > maxFreq) records[i].freq = maxFreq;
            }
        }
    }
}